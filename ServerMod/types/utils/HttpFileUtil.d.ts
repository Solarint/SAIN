/// <reference types="node" />
import { ServerResponse } from "node:http";
import { HttpServerHelper } from "@spt-aki/helpers/HttpServerHelper";
export declare class HttpFileUtil {
    protected httpServerHelper: HttpServerHelper;
    constructor(httpServerHelper: HttpServerHelper);
    sendFile(resp: ServerResponse, file: any): void;
}
