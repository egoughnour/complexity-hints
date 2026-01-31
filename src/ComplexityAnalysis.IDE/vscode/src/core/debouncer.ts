/**
 * Debouncer utility for delaying execution until input stops.
 * Cancels previous pending work when new work is scheduled.
 */

export class Debouncer {
    private _timer: NodeJS.Timeout | null = null;
    private _abortController: AbortController | null = null;

    constructor(private readonly _delayMs: number) {}

    /**
     * Schedules work to run after the debounce delay.
     * Cancels any previously scheduled work.
     * 
     * @param work The async function to run
     * @returns A promise that resolves when the work completes (or rejects if cancelled)
     */
    async debounce<T>(work: (signal: AbortSignal) => Promise<T>): Promise<T | null> {
        // Cancel any pending work
        this.cancel();

        // Create new abort controller for this work
        this._abortController = new AbortController();
        const signal = this._abortController.signal;

        return new Promise<T | null>((resolve, reject) => {
            this._timer = setTimeout(async () => {
                if (signal.aborted) {
                    resolve(null);
                    return;
                }

                try {
                    const result = await work(signal);
                    if (!signal.aborted) {
                        resolve(result);
                    } else {
                        resolve(null);
                    }
                } catch (error) {
                    if (!signal.aborted) {
                        reject(error);
                    } else {
                        resolve(null);
                    }
                } finally {
                    this._timer = null;
                    this._abortController = null;
                }
            }, this._delayMs);
        });
    }

    /**
     * Schedules work without waiting for the result.
     * Fire-and-forget pattern.
     */
    debounceVoid(work: (signal: AbortSignal) => Promise<void>): void {
        this.debounce(work).catch(() => {
            // Ignore errors in fire-and-forget mode
        });
    }

    /**
     * Cancels any pending work.
     */
    cancel(): void {
        if (this._timer) {
            clearTimeout(this._timer);
            this._timer = null;
        }

        if (this._abortController) {
            this._abortController.abort();
            this._abortController = null;
        }
    }

    /**
     * Whether there is pending work.
     */
    get isPending(): boolean {
        return this._timer !== null;
    }

    /**
     * Disposes the debouncer.
     */
    dispose(): void {
        this.cancel();
    }
}

/**
 * A collection of debouncers keyed by string.
 * Useful for per-document debouncing.
 */
export class DebouncerMap {
    private readonly _debouncers = new Map<string, Debouncer>();

    constructor(private readonly _delayMs: number) {}

    /**
     * Gets or creates a debouncer for the given key.
     */
    get(key: string): Debouncer {
        let debouncer = this._debouncers.get(key);
        if (!debouncer) {
            debouncer = new Debouncer(this._delayMs);
            this._debouncers.set(key, debouncer);
        }
        return debouncer;
    }

    /**
     * Removes a debouncer for the given key.
     */
    remove(key: string): void {
        const debouncer = this._debouncers.get(key);
        if (debouncer) {
            debouncer.dispose();
            this._debouncers.delete(key);
        }
    }

    /**
     * Cancels all pending work.
     */
    cancelAll(): void {
        for (const debouncer of this._debouncers.values()) {
            debouncer.cancel();
        }
    }

    /**
     * Disposes all debouncers.
     */
    dispose(): void {
        for (const debouncer of this._debouncers.values()) {
            debouncer.dispose();
        }
        this._debouncers.clear();
    }
}
