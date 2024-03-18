import { DependencyContainer } from "tsyringe";
import { BundleLoader } from "@spt-aki/loaders/BundleLoader";
import { ModTypeCheck } from "@spt-aki/loaders/ModTypeCheck";
import { PreAkiModLoader } from "@spt-aki/loaders/PreAkiModLoader";
import { IModLoader } from "@spt-aki/models/spt/mod/IModLoader";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { VFS } from "@spt-aki/utils/VFS";
export declare class PostAkiModLoader implements IModLoader {
    protected logger: ILogger;
    protected bundleLoader: BundleLoader;
    protected vfs: VFS;
    protected preAkiModLoader: PreAkiModLoader;
    protected localisationService: LocalisationService;
    protected modTypeCheck: ModTypeCheck;
    constructor(logger: ILogger, bundleLoader: BundleLoader, vfs: VFS, preAkiModLoader: PreAkiModLoader, localisationService: LocalisationService, modTypeCheck: ModTypeCheck);
    getModPath(mod: string): string;
    load(): Promise<void>;
    protected executeMods(container: DependencyContainer): Promise<void>;
    protected addBundles(): void;
}
