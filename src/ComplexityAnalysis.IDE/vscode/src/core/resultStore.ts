/**
 * Result store for caching complexity analysis results.
 * Thread-safe (single-threaded JS) cache with pub/sub notifications.
 */

import { ComplexityHint, CacheKey, MethodHeaderInfo } from './types';

type ChangeCallback = () => void;

/**
 * Stores complexity analysis results with document-aware caching.
 */
export class ResultStore {
    /** Hints by method ID */
    private readonly _hints = new Map<string, ComplexityHint>();
    
    /** Method headers by document URI */
    private readonly _methods = new Map<string, MethodHeaderInfo[]>();
    
    /** Document versions by URI */
    private readonly _versions = new Map<string, number>();
    
    /** Subscribers by method ID */
    private readonly _subscribers = new Map<string, Set<ChangeCallback>>();
    
    /** Global subscribers notified on any change */
    private readonly _globalSubscribers = new Set<ChangeCallback>();

    /**
     * Gets a hint by method ID.
     */
    getHint(methodId: string): ComplexityHint | undefined {
        return this._hints.get(methodId);
    }

    /**
     * Gets a hint if it matches the document version.
     */
    getHintIfCurrent(methodId: string, documentUri: string, documentVersion: number): ComplexityHint | undefined {
        const hint = this._hints.get(methodId);
        if (!hint) {
            return undefined;
        }

        const storedVersion = this._versions.get(documentUri);
        if (storedVersion !== documentVersion) {
            return undefined;
        }

        return hint;
    }

    /**
     * Gets all hints for a document.
     */
    getHintsForDocument(documentUri: string): ComplexityHint[] {
        const methods = this._methods.get(documentUri);
        if (!methods) {
            return [];
        }

        const hints: ComplexityHint[] = [];
        for (const method of methods) {
            const hint = this._hints.get(method.methodId);
            if (hint) {
                hints.push(hint);
            }
        }
        return hints;
    }

    /**
     * Gets method headers for a document.
     */
    getMethodsForDocument(documentUri: string): MethodHeaderInfo[] {
        return this._methods.get(documentUri) || [];
    }

    /**
     * Publishes a hint, notifying subscribers.
     */
    publishHint(hint: ComplexityHint): void {
        this._hints.set(hint.methodId, hint);
        this.notifySubscribers(hint.methodId);
        this.notifyGlobalSubscribers();
    }

    /**
     * Publishes multiple hints at once.
     */
    publishHints(hints: ComplexityHint[]): void {
        for (const hint of hints) {
            this._hints.set(hint.methodId, hint);
            this.notifySubscribers(hint.methodId);
        }
        this.notifyGlobalSubscribers();
    }

    /**
     * Updates method headers for a document.
     */
    setMethodsForDocument(documentUri: string, methods: MethodHeaderInfo[], version: number): void {
        // Remove old hints for methods that no longer exist
        const oldMethods = this._methods.get(documentUri);
        if (oldMethods) {
            const newMethodIds = new Set(methods.map(m => m.methodId));
            for (const oldMethod of oldMethods) {
                if (!newMethodIds.has(oldMethod.methodId)) {
                    this._hints.delete(oldMethod.methodId);
                }
            }
        }

        this._methods.set(documentUri, methods);
        this._versions.set(documentUri, version);
        this.notifyGlobalSubscribers();
    }

    /**
     * Removes all data for a document.
     */
    removeDocument(documentUri: string): void {
        const methods = this._methods.get(documentUri);
        if (methods) {
            for (const method of methods) {
                this._hints.delete(method.methodId);
            }
        }
        this._methods.delete(documentUri);
        this._versions.delete(documentUri);
        this.notifyGlobalSubscribers();
    }

    /**
     * Clears all cached data.
     */
    clear(): void {
        this._hints.clear();
        this._methods.clear();
        this._versions.clear();
        this.notifyGlobalSubscribers();
    }

    /**
     * Subscribes to changes for a specific method.
     * Returns an unsubscribe function.
     */
    subscribe(methodId: string, callback: ChangeCallback): () => void {
        let subscribers = this._subscribers.get(methodId);
        if (!subscribers) {
            subscribers = new Set();
            this._subscribers.set(methodId, subscribers);
        }
        subscribers.add(callback);

        return () => {
            const subs = this._subscribers.get(methodId);
            if (subs) {
                subs.delete(callback);
                if (subs.size === 0) {
                    this._subscribers.delete(methodId);
                }
            }
        };
    }

    /**
     * Subscribes to any changes in the store.
     * Returns an unsubscribe function.
     */
    subscribeGlobal(callback: ChangeCallback): () => void {
        this._globalSubscribers.add(callback);
        return () => {
            this._globalSubscribers.delete(callback);
        };
    }

    /**
     * Gets statistics about the cache.
     */
    getStats(): { hintCount: number; documentCount: number; methodCount: number } {
        let methodCount = 0;
        for (const methods of this._methods.values()) {
            methodCount += methods.length;
        }

        return {
            hintCount: this._hints.size,
            documentCount: this._methods.size,
            methodCount
        };
    }

    private notifySubscribers(methodId: string): void {
        const subscribers = this._subscribers.get(methodId);
        if (subscribers) {
            for (const callback of subscribers) {
                try {
                    callback();
                } catch {
                    // Ignore errors in callbacks
                }
            }
        }
    }

    private notifyGlobalSubscribers(): void {
        for (const callback of this._globalSubscribers) {
            try {
                callback();
            } catch {
                // Ignore errors in callbacks
            }
        }
    }
}
