import { DependencyContainer } from "tsyringe";
import { OnLoad } from "@spt-aki/di/OnLoad";
import { ModTypeCheck } from "@spt-aki/loaders/ModTypeCheck";
import { PreAkiModLoader } from "@spt-aki/loaders/PreAkiModLoader";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
export declare class PostDBModLoader implements OnLoad {
    protected logger: ILogger;
    protected preAkiModLoader: PreAkiModLoader;
    protected localisationService: LocalisationService;
    protected modTypeCheck: ModTypeCheck;
    constructor(logger: ILogger, preAkiModLoader: PreAkiModLoader, localisationService: LocalisationService, modTypeCheck: ModTypeCheck);
    onLoad(): Promise<void>;
    getRoute(): string;
    getModPath(mod: string): string;
    protected executeMods(container: DependencyContainer): Promise<void>;
}
