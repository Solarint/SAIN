import { IAsyncQueue } from "@spt-aki/models/spt/utils/IAsyncQueue";
import { IUUidGenerator } from "@spt-aki/models/spt/utils/IUuidGenerator";
import { AbstractWinstonLogger } from "@spt-aki/utils/logging/AbstractWinstonLogger";
export declare class WinstonRequestLogger extends AbstractWinstonLogger {
    protected asyncQueue: IAsyncQueue;
    protected uuidGenerator: IUUidGenerator;
    constructor(asyncQueue: IAsyncQueue, uuidGenerator: IUUidGenerator);
    protected isLogExceptions(): boolean;
    protected isLogToFile(): boolean;
    protected isLogToConsole(): boolean;
    protected getFilePath(): string;
    protected getFileName(): string;
    protected getLogMaxSize(): string;
}
