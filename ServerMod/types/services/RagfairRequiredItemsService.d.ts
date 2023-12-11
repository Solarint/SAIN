import { PaymentHelper } from "@spt-aki/helpers/PaymentHelper";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { RagfairOfferService } from "@spt-aki/services/RagfairOfferService";
export declare class RagfairRequiredItemsService {
    protected logger: ILogger;
    protected paymentHelper: PaymentHelper;
    protected ragfairOfferService: RagfairOfferService;
    protected requiredItemsCache: {};
    constructor(logger: ILogger, paymentHelper: PaymentHelper, ragfairOfferService: RagfairOfferService);
    getRequiredItemsById(searchId: string): any;
    buildRequiredItemTable(): void;
}
