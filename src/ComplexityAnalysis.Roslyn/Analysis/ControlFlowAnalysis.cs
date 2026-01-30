using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Builds and analyzes control flow graphs for complexity analysis.
/// </summary>
public sealed class ControlFlowAnalysis
{
    private readonly SemanticModel _semanticModel;

    public ControlFlowAnalysis(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Analyzes the control flow of a method body.
    /// </summary>
    public ControlFlowResult AnalyzeMethod(MethodDeclarationSyntax method)
    {
        if (method.Body is null && method.ExpressionBody is null)
        {
            return new ControlFlowResult
            {
                Success = false,
                ErrorMessage = "Method has no body"
            };
        }

        var cfg = BuildControlFlowGraph(method);
        if (cfg is null)
        {
            return new ControlFlowResult
            {
                Success = false,
                ErrorMessage = "Could not build control flow graph"
            };
        }

        // Use AST-based nesting depth as primary (more reliable than CFG analysis)
        var astNestingDepth = ComputeLoopNestingDepthFromSyntax(method);
        var cfgNestingDepth = ComputeLoopNestingDepth(cfg);

        // Use the maximum of both calculations to avoid underreporting
        var nestingDepth = Math.Max(astNestingDepth, cfgNestingDepth);

        return new ControlFlowResult
        {
            Success = true,
            Graph = cfg,
            IsReducible = IsReducible(cfg),
            LoopNestingDepth = nestingDepth,
            CyclomaticComplexity = ComputeCyclomaticComplexity(cfg),
            BranchingFactor = ComputeBranchingFactor(cfg)
        };
    }

    /// <summary>
    /// Builds a simplified control flow graph.
    /// </summary>
    public SimplifiedCFG? BuildControlFlowGraph(MethodDeclarationSyntax method)
    {
        try
        {
            // Use Roslyn's built-in CFG if the method has a body
            if (method.Body is not null)
            {
                var cfg = Microsoft.CodeAnalysis.FlowAnalysis.ControlFlowGraph.Create(method.Body, _semanticModel);
                return ConvertToSimplifiedCFG(cfg, method);
            }

            if (method.ExpressionBody is not null)
            {
                // Expression-bodied members are single basic blocks
                return new SimplifiedCFG
                {
                    EntryBlock = new CFGBlock(0, CFGBlockKind.Entry),
                    ExitBlock = new CFGBlock(1, CFGBlockKind.Exit),
                    Blocks = ImmutableList.Create(
                        new CFGBlock(0, CFGBlockKind.Entry),
                        new CFGBlock(1, CFGBlockKind.Exit)
                    ),
                    Edges = ImmutableList.Create(
                        new CFGEdge(0, 1, CFGEdgeKind.Normal)
                    )
                };
            }

            return null;
        }
        catch
        {
            // Fallback to manual construction if Roslyn CFG fails
            return BuildManualCFG(method);
        }
    }

    private SimplifiedCFG? ConvertToSimplifiedCFG(ControlFlowGraph roslynCfg, MethodDeclarationSyntax method)
    {
        var blocks = new List<CFGBlock>();
        var edges = new List<CFGEdge>();
        var blockMap = new Dictionary<BasicBlock, int>();

        // Collect all loop statement locations from syntax
        var loopStatements = CollectLoopStatements(method);

        // Map Roslyn blocks to our simplified blocks
        int blockId = 0;
        foreach (var block in roslynCfg.Blocks)
        {
            blockMap[block] = blockId;
            var isLoopHeader = IsBlockLoopHeader(block, loopStatements);
            var kind = block.Kind switch
            {
                BasicBlockKind.Entry => CFGBlockKind.Entry,
                BasicBlockKind.Exit => CFGBlockKind.Exit,
                BasicBlockKind.Block when isLoopHeader => CFGBlockKind.LoopHeader,
                BasicBlockKind.Block => DetermineBlockKind(block),
                _ => CFGBlockKind.Normal
            };

            var cfgBlock = new CFGBlock(blockId, kind)
            {
                Statements = ExtractStatements(block, method),
                IsLoopHeader = isLoopHeader,
                IsConditional = block.ConditionalSuccessor is not null
            };

            blocks.Add(cfgBlock);
            blockId++;
        }

        // Build edges
        foreach (var block in roslynCfg.Blocks)
        {
            var sourceId = blockMap[block];

            if (block.FallThroughSuccessor?.Destination is not null)
            {
                var destId = blockMap[block.FallThroughSuccessor.Destination];
                edges.Add(new CFGEdge(sourceId, destId, CFGEdgeKind.Normal));
            }

            if (block.ConditionalSuccessor?.Destination is not null)
            {
                var destId = blockMap[block.ConditionalSuccessor.Destination];
                edges.Add(new CFGEdge(sourceId, destId, CFGEdgeKind.Conditional));
            }
        }

        var entryBlock = blocks.FirstOrDefault(b => b.Kind == CFGBlockKind.Entry);
        var exitBlock = blocks.FirstOrDefault(b => b.Kind == CFGBlockKind.Exit);

        if (entryBlock is null || exitBlock is null)
            return null;

        return new SimplifiedCFG
        {
            EntryBlock = entryBlock,
            ExitBlock = exitBlock,
            Blocks = blocks.ToImmutableList(),
            Edges = edges.ToImmutableList()
        };
    }

    private static HashSet<SyntaxNode> CollectLoopStatements(MethodDeclarationSyntax method)
    {
        var loops = new HashSet<SyntaxNode>();

        if (method.Body is not null)
        {
            foreach (var node in method.Body.DescendantNodes())
            {
                if (node is ForStatementSyntax or
                    WhileStatementSyntax or
                    DoStatementSyntax or
                    ForEachStatementSyntax)
                {
                    loops.Add(node);
                }
            }
        }

        return loops;
    }

    private static bool IsBlockLoopHeader(BasicBlock block, HashSet<SyntaxNode> loopStatements)
    {
        // A block is a loop header if any of its operations' syntax is a loop statement
        // or if the block's branch value syntax is part of a loop condition
        foreach (var operation in block.Operations)
        {
            if (operation.Syntax is not null)
            {
                // Check if this operation is directly a loop or is the condition of a loop
                var syntax = operation.Syntax;
                if (loopStatements.Contains(syntax))
                    return true;

                // Check if the syntax is part of a loop's condition
                var parent = syntax.Parent;
                while (parent is not null)
                {
                    if (loopStatements.Contains(parent))
                    {
                        // Check if we're in the condition part (not the body)
                        if (parent is ForStatementSyntax forStmt && forStmt.Condition?.Contains(syntax) == true)
                            return true;
                        if (parent is WhileStatementSyntax whileStmt && whileStmt.Condition.Contains(syntax))
                            return true;
                        if (parent is DoStatementSyntax doStmt && doStmt.Condition.Contains(syntax))
                            return true;
                        if (parent is ForEachStatementSyntax)
                            return true; // foreach header includes the iteration setup
                        break;
                    }
                    parent = parent.Parent;
                }
            }
        }

        // Also check the branch value (the condition being evaluated)
        if (block.BranchValue?.Syntax is { } branchSyntax)
        {
            var parent = branchSyntax.Parent;
            while (parent is not null)
            {
                if (loopStatements.Contains(parent))
                    return true;
                parent = parent.Parent;
            }
        }

        return false;
    }

    private SimplifiedCFG? BuildManualCFG(MethodDeclarationSyntax method)
    {
        if (method.Body is null)
            return null;

        var builder = new ManualCFGBuilder(_semanticModel);
        return builder.Build(method.Body);
    }

    private CFGBlockKind DetermineBlockKind(BasicBlock block)
    {
        // Check if this block contains loop-related operations
        foreach (var operation in block.Operations)
        {
            if (operation.Syntax is ForStatementSyntax or
                ForEachStatementSyntax or
                WhileStatementSyntax or
                DoStatementSyntax)
            {
                return CFGBlockKind.LoopHeader;
            }
        }

        if (block.ConditionalSuccessor is not null)
            return CFGBlockKind.Conditional;

        return CFGBlockKind.Normal;
    }

    private ImmutableList<SyntaxNode> ExtractStatements(BasicBlock block, MethodDeclarationSyntax method)
    {
        var statements = new List<SyntaxNode>();

        foreach (var operation in block.Operations)
        {
            if (operation.Syntax is not null)
            {
                statements.Add(operation.Syntax);
            }
        }

        return statements.ToImmutableList();
    }

    private bool IsLoopHeader(BasicBlock block)
    {
        foreach (var operation in block.Operations)
        {
            if (operation.Syntax is ForStatementSyntax or
                ForEachStatementSyntax or
                WhileStatementSyntax or
                DoStatementSyntax)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the CFG is reducible (has structured control flow).
    /// </summary>
    public bool IsReducible(SimplifiedCFG cfg)
    {
        // A CFG is reducible if all back edges go to loop headers
        // and there are no irreducible cycles

        var dominators = ComputeDominators(cfg);
        var backEdges = FindBackEdges(cfg, dominators);

        // Check that all back edges target a dominator
        foreach (var edge in backEdges)
        {
            var sourceBlock = cfg.Blocks.First(b => b.Id == edge.Source);
            var targetBlock = cfg.Blocks.First(b => b.Id == edge.Target);

            if (!Dominates(dominators, targetBlock.Id, sourceBlock.Id))
            {
                return false; // Irreducible
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the maximum loop nesting depth.
    /// Uses both CFG-based analysis and AST-based fallback for accuracy.
    /// </summary>
    public int ComputeLoopNestingDepth(SimplifiedCFG cfg)
    {
        var dominators = ComputeDominators(cfg);
        var backEdges = FindBackEdges(cfg, dominators);

        // Find natural loops
        var loops = new List<(int header, HashSet<int> body)>();
        foreach (var edge in backEdges)
        {
            var loop = FindNaturalLoop(cfg, edge.Target, edge.Source);
            loops.Add((edge.Target, loop));
        }

        // Compute nesting
        if (loops.Count == 0)
            return 0;

        var maxDepth = 1;
        for (int i = 0; i < loops.Count; i++)
        {
            var depth = 1;
            for (int j = 0; j < loops.Count; j++)
            {
                if (i != j && loops[j].body.Contains(loops[i].header))
                {
                    depth++;
                }
            }
            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    /// <summary>
    /// Computes loop nesting depth directly from AST (more reliable than CFG analysis).
    /// </summary>
    public static int ComputeLoopNestingDepthFromSyntax(MethodDeclarationSyntax method)
    {
        if (method.Body is null)
            return 0;

        return ComputeMaxNestingDepthRecursive(method.Body, 0);
    }

    private static int ComputeMaxNestingDepthRecursive(SyntaxNode node, int currentDepth)
    {
        var maxDepth = currentDepth;

        foreach (var child in node.ChildNodes())
        {
            var childDepth = child switch
            {
                ForStatementSyntax forLoop => Math.Max(
                    ComputeMaxNestingDepthRecursive(forLoop.Statement, currentDepth + 1),
                    currentDepth + 1),
                WhileStatementSyntax whileLoop => Math.Max(
                    ComputeMaxNestingDepthRecursive(whileLoop.Statement, currentDepth + 1),
                    currentDepth + 1),
                DoStatementSyntax doLoop => Math.Max(
                    ComputeMaxNestingDepthRecursive(doLoop.Statement, currentDepth + 1),
                    currentDepth + 1),
                ForEachStatementSyntax foreachLoop => Math.Max(
                    ComputeMaxNestingDepthRecursive(foreachLoop.Statement, currentDepth + 1),
                    currentDepth + 1),
                _ => ComputeMaxNestingDepthRecursive(child, currentDepth)
            };

            maxDepth = Math.Max(maxDepth, childDepth);
        }

        return maxDepth;
    }

    /// <summary>
    /// Computes cyclomatic complexity: E - N + 2P
    /// </summary>
    public int ComputeCyclomaticComplexity(SimplifiedCFG cfg)
    {
        var edges = cfg.Edges.Count;
        var nodes = cfg.Blocks.Count;
        var connectedComponents = 1; // Assuming single entry point

        return edges - nodes + 2 * connectedComponents;
    }

    /// <summary>
    /// Computes the average branching factor.
    /// </summary>
    public double ComputeBranchingFactor(SimplifiedCFG cfg)
    {
        var totalOutEdges = cfg.Blocks.Sum(b =>
            cfg.Edges.Count(e => e.Source == b.Id));
        return (double)totalOutEdges / cfg.Blocks.Count;
    }

    private Dictionary<int, int> ComputeDominators(SimplifiedCFG cfg)
    {
        // Simplified dominator computation
        var dominators = new Dictionary<int, int>();
        var entry = cfg.EntryBlock.Id;

        dominators[entry] = entry;

        // Initialize all other blocks to be dominated by entry
        foreach (var block in cfg.Blocks)
        {
            if (block.Id != entry)
                dominators[block.Id] = entry;
        }

        // Iterative dominator computation
        bool changed;
        do
        {
            changed = false;
            foreach (var block in cfg.Blocks)
            {
                if (block.Id == entry) continue;

                var predecessors = cfg.Edges
                    .Where(e => e.Target == block.Id)
                    .Select(e => e.Source)
                    .ToList();

                if (predecessors.Count > 0)
                {
                    var newDom = predecessors[0];
                    for (int i = 1; i < predecessors.Count; i++)
                    {
                        newDom = Intersect(dominators, newDom, predecessors[i]);
                    }

                    if (newDom != dominators[block.Id])
                    {
                        dominators[block.Id] = newDom;
                        changed = true;
                    }
                }
            }
        } while (changed);

        return dominators;
    }

    private int Intersect(Dictionary<int, int> dominators, int b1, int b2)
    {
        while (b1 != b2)
        {
            while (b1 > b2)
                b1 = dominators[b1];
            while (b2 > b1)
                b2 = dominators[b2];
        }
        return b1;
    }

    private bool Dominates(Dictionary<int, int> dominators, int dominator, int block)
    {
        var current = block;
        while (current != dominators[current])
        {
            if (current == dominator)
                return true;
            current = dominators[current];
        }
        return current == dominator;
    }

    private IReadOnlyList<CFGEdge> FindBackEdges(SimplifiedCFG cfg, Dictionary<int, int> dominators)
    {
        var backEdges = new List<CFGEdge>();

        foreach (var edge in cfg.Edges)
        {
            if (Dominates(dominators, edge.Target, edge.Source))
            {
                backEdges.Add(edge);
            }
        }

        return backEdges;
    }

    private HashSet<int> FindNaturalLoop(SimplifiedCFG cfg, int header, int tail)
    {
        var loop = new HashSet<int> { header };
        var workList = new Stack<int>();

        if (header != tail)
        {
            loop.Add(tail);
            workList.Push(tail);
        }

        while (workList.Count > 0)
        {
            var node = workList.Pop();
            foreach (var edge in cfg.Edges.Where(e => e.Target == node))
            {
                if (loop.Add(edge.Source))
                {
                    workList.Push(edge.Source);
                }
            }
        }

        return loop;
    }

    /// <summary>
    /// Manual CFG builder for when Roslyn's CFG is unavailable.
    /// </summary>
    private class ManualCFGBuilder : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly List<CFGBlock> _blocks = new();
        private readonly List<CFGEdge> _edges = new();
        private int _nextBlockId = 0;
        private CFGBlock? _currentBlock;

        public ManualCFGBuilder(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public SimplifiedCFG Build(BlockSyntax body)
        {
            // Create entry block
            var entry = CreateBlock(CFGBlockKind.Entry);
            _currentBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(entry.Id, _currentBlock.Id, CFGEdgeKind.Normal));

            Visit(body);

            // Create exit block
            var exit = CreateBlock(CFGBlockKind.Exit);
            if (_currentBlock is not null)
            {
                _edges.Add(new CFGEdge(_currentBlock.Id, exit.Id, CFGEdgeKind.Normal));
            }

            return new SimplifiedCFG
            {
                EntryBlock = entry,
                ExitBlock = exit,
                Blocks = _blocks.ToImmutableList(),
                Edges = _edges.ToImmutableList()
            };
        }

        private CFGBlock CreateBlock(CFGBlockKind kind, bool isLoopHeader = false, bool isConditional = false)
        {
            var block = new CFGBlock(_nextBlockId++, kind)
            {
                IsLoopHeader = isLoopHeader,
                IsConditional = isConditional
            };
            _blocks.Add(block);
            return block;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            CFGBlock conditionBlock;
            if (_currentBlock is not null)
            {
                // Mark the current block as conditional
                var index = _blocks.IndexOf(_currentBlock);
                conditionBlock = _currentBlock with { IsConditional = true };
                if (index >= 0)
                    _blocks[index] = conditionBlock;
            }
            else
            {
                conditionBlock = CreateBlock(CFGBlockKind.Conditional, isConditional: true);
            }

            var trueBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(conditionBlock.Id, trueBlock.Id, CFGEdgeKind.Conditional));

            _currentBlock = trueBlock;
            Visit(node.Statement);
            var afterTrue = _currentBlock;

            CFGBlock? afterFalse = null;
            if (node.Else is not null)
            {
                var falseBlock = CreateBlock(CFGBlockKind.Normal);
                _edges.Add(new CFGEdge(conditionBlock.Id, falseBlock.Id, CFGEdgeKind.Normal));

                _currentBlock = falseBlock;
                Visit(node.Else.Statement);
                afterFalse = _currentBlock;
            }

            // Merge point
            var mergeBlock = CreateBlock(CFGBlockKind.Normal);
            if (afterTrue is not null)
                _edges.Add(new CFGEdge(afterTrue.Id, mergeBlock.Id, CFGEdgeKind.Normal));
            if (afterFalse is not null)
                _edges.Add(new CFGEdge(afterFalse.Id, mergeBlock.Id, CFGEdgeKind.Normal));
            else
                _edges.Add(new CFGEdge(conditionBlock.Id, mergeBlock.Id, CFGEdgeKind.Normal));

            _currentBlock = mergeBlock;
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            var headerBlock = CreateBlock(CFGBlockKind.LoopHeader, isLoopHeader: true);

            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.Normal));

            var bodyBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, bodyBlock.Id, CFGEdgeKind.Conditional));

            _currentBlock = bodyBlock;
            Visit(node.Statement);

            // Back edge
            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.BackEdge));

            // Exit edge
            var exitBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, exitBlock.Id, CFGEdgeKind.Normal));
            _currentBlock = exitBlock;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            var headerBlock = CreateBlock(CFGBlockKind.LoopHeader, isLoopHeader: true);

            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.Normal));

            var bodyBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, bodyBlock.Id, CFGEdgeKind.Conditional));

            _currentBlock = bodyBlock;
            Visit(node.Statement);

            // Back edge
            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.BackEdge));

            // Exit edge
            var exitBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, exitBlock.Id, CFGEdgeKind.Normal));
            _currentBlock = exitBlock;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            var headerBlock = CreateBlock(CFGBlockKind.LoopHeader, isLoopHeader: true);

            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.Normal));

            var bodyBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, bodyBlock.Id, CFGEdgeKind.Conditional));

            _currentBlock = bodyBlock;
            Visit(node.Statement);

            // Back edge
            if (_currentBlock is not null)
                _edges.Add(new CFGEdge(_currentBlock.Id, headerBlock.Id, CFGEdgeKind.BackEdge));

            // Exit edge
            var exitBlock = CreateBlock(CFGBlockKind.Normal);
            _edges.Add(new CFGEdge(headerBlock.Id, exitBlock.Id, CFGEdgeKind.Normal));
            _currentBlock = exitBlock;
        }
    }
}

/// <summary>
/// Result of control flow analysis.
/// </summary>
public record ControlFlowResult
{
    /// <summary>
    /// Whether the analysis was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The control flow graph.
    /// </summary>
    public SimplifiedCFG? Graph { get; init; }

    /// <summary>
    /// Whether the CFG is reducible (structured control flow).
    /// </summary>
    public bool IsReducible { get; init; }

    /// <summary>
    /// Maximum loop nesting depth.
    /// </summary>
    public int LoopNestingDepth { get; init; }

    /// <summary>
    /// Cyclomatic complexity (E - N + 2P).
    /// </summary>
    public int CyclomaticComplexity { get; init; }

    /// <summary>
    /// Average branching factor.
    /// </summary>
    public double BranchingFactor { get; init; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Simplified control flow graph representation.
/// </summary>
public record SimplifiedCFG
{
    /// <summary>
    /// The entry block.
    /// </summary>
    public required CFGBlock EntryBlock { get; init; }

    /// <summary>
    /// The exit block.
    /// </summary>
    public required CFGBlock ExitBlock { get; init; }

    /// <summary>
    /// All basic blocks.
    /// </summary>
    public required ImmutableList<CFGBlock> Blocks { get; init; }

    /// <summary>
    /// All edges between blocks.
    /// </summary>
    public required ImmutableList<CFGEdge> Edges { get; init; }

    /// <summary>
    /// Gets successors of a block.
    /// </summary>
    public IEnumerable<CFGBlock> GetSuccessors(CFGBlock block) =>
        Edges.Where(e => e.Source == block.Id)
            .Select(e => Blocks.First(b => b.Id == e.Target));

    /// <summary>
    /// Gets predecessors of a block.
    /// </summary>
    public IEnumerable<CFGBlock> GetPredecessors(CFGBlock block) =>
        Edges.Where(e => e.Target == block.Id)
            .Select(e => Blocks.First(b => b.Id == e.Source));

    /// <summary>
    /// Finds all loop headers.
    /// </summary>
    public IEnumerable<CFGBlock> LoopHeaders =>
        Blocks.Where(b => b.IsLoopHeader);
}

/// <summary>
/// A basic block in the CFG.
/// </summary>
public record CFGBlock
{
    public int Id { get; }
    public CFGBlockKind Kind { get; init; }
    public ImmutableList<SyntaxNode> Statements { get; init; } = ImmutableList<SyntaxNode>.Empty;
    public bool IsLoopHeader { get; init; }
    public bool IsConditional { get; init; }

    public CFGBlock(int id, CFGBlockKind kind)
    {
        Id = id;
        Kind = kind;
    }
}

/// <summary>
/// Kind of CFG block.
/// </summary>
public enum CFGBlockKind
{
    Entry,
    Exit,
    Normal,
    LoopHeader,
    Conditional
}

/// <summary>
/// An edge in the CFG.
/// </summary>
public record CFGEdge(int Source, int Target, CFGEdgeKind Kind);

/// <summary>
/// Kind of CFG edge.
/// </summary>
public enum CFGEdgeKind
{
    Normal,
    Conditional,
    BackEdge,
    Exception
}
