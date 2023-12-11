import { RagfairOfferGenerator } from "@spt-aki/generators/RagfairOfferGenerator";
import { HandbookHelper } from "@spt-aki/helpers/HandbookHelper";
import { InventoryHelper } from "@spt-aki/helpers/InventoryHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { PaymentHelper } from "@spt-aki/helpers/PaymentHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { RagfairHelper } from "@spt-aki/helpers/RagfairHelper";
import { RagfairOfferHelper } from "@spt-aki/helpers/RagfairOfferHelper";
import { RagfairSellHelper } from "@spt-aki/helpers/RagfairSellHelper";
import { RagfairSortHelper } from "@spt-aki/helpers/RagfairSortHelper";
import { TraderHelper } from "@spt-aki/helpers/TraderHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { ITraderAssort } from "@spt-aki/models/eft/common/tables/ITrader";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { IAkiProfile } from "@spt-aki/models/eft/profile/IAkiProfile";
import { IAddOfferRequestData, Requirement } from "@spt-aki/models/eft/ragfair/IAddOfferRequestData";
import { IExtendOfferRequestData } from "@spt-aki/models/eft/ragfair/IExtendOfferRequestData";
import { IGetItemPriceResult } from "@spt-aki/models/eft/ragfair/IGetItemPriceResult";
import { IGetMarketPriceRequestData } from "@spt-aki/models/eft/ragfair/IGetMarketPriceRequestData";
import { IGetOffersResult } from "@spt-aki/models/eft/ragfair/IGetOffersResult";
import { IRagfairOffer } from "@spt-aki/models/eft/ragfair/IRagfairOffer";
import { ISearchRequestData } from "@spt-aki/models/eft/ragfair/ISearchRequestData";
import { IProcessBuyTradeRequestData } from "@spt-aki/models/eft/trade/IProcessBuyTradeRequestData";
import { IRagfairConfig } from "@spt-aki/models/spt/config/IRagfairConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { EventOutputHolder } from "@spt-aki/routers/EventOutputHolder";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { RagfairServer } from "@spt-aki/servers/RagfairServer";
import { SaveServer } from "@spt-aki/servers/SaveServer";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { PaymentService } from "@spt-aki/services/PaymentService";
import { RagfairOfferService } from "@spt-aki/services/RagfairOfferService";
import { RagfairPriceService } from "@spt-aki/services/RagfairPriceService";
import { RagfairRequiredItemsService } from "@spt-aki/services/RagfairRequiredItemsService";
import { RagfairTaxService } from "@spt-aki/services/RagfairTaxService";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
import { TimeUtil } from "@spt-aki/utils/TimeUtil";
/**
 * Handle RagfairCallback events
 */
export declare class RagfairController {
    protected logger: ILogger;
    protected timeUtil: TimeUtil;
    protected httpResponse: HttpResponseUtil;
    protected eventOutputHolder: EventOutputHolder;
    protected ragfairServer: RagfairServer;
    protected ragfairPriceService: RagfairPriceService;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected saveServer: SaveServer;
    protected ragfairSellHelper: RagfairSellHelper;
    protected ragfairTaxService: RagfairTaxService;
    protected ragfairSortHelper: RagfairSortHelper;
    protected ragfairOfferHelper: RagfairOfferHelper;
    protected profileHelper: ProfileHelper;
    protected paymentService: PaymentService;
    protected handbookHelper: HandbookHelper;
    protected paymentHelper: PaymentHelper;
    protected inventoryHelper: InventoryHelper;
    protected traderHelper: TraderHelper;
    protected ragfairHelper: RagfairHelper;
    protected ragfairOfferService: RagfairOfferService;
    protected ragfairRequiredItemsService: RagfairRequiredItemsService;
    protected ragfairOfferGenerator: RagfairOfferGenerator;
    protected localisationService: LocalisationService;
    protected configServer: ConfigServer;
    protected ragfairConfig: IRagfairConfig;
    constructor(logger: ILogger, timeUtil: TimeUtil, httpResponse: HttpResponseUtil, eventOutputHolder: EventOutputHolder, ragfairServer: RagfairServer, ragfairPriceService: RagfairPriceService, databaseServer: DatabaseServer, itemHelper: ItemHelper, saveServer: SaveServer, ragfairSellHelper: RagfairSellHelper, ragfairTaxService: RagfairTaxService, ragfairSortHelper: RagfairSortHelper, ragfairOfferHelper: RagfairOfferHelper, profileHelper: ProfileHelper, paymentService: PaymentService, handbookHelper: HandbookHelper, paymentHelper: PaymentHelper, inventoryHelper: InventoryHelper, traderHelper: TraderHelper, ragfairHelper: RagfairHelper, ragfairOfferService: RagfairOfferService, ragfairRequiredItemsService: RagfairRequiredItemsService, ragfairOfferGenerator: RagfairOfferGenerator, localisationService: LocalisationService, configServer: ConfigServer);
    getOffers(sessionID: string, searchRequest: ISearchRequestData): IGetOffersResult;
    /**
     * Get offers for the client based on type of search being performed
     * @param searchRequest Client search request data
     * @param itemsToAdd comes from ragfairHelper.filterCategories()
     * @param traderAssorts Trader assorts
     * @param pmcProfile Player profile
     * @returns array of offers
     */
    protected getOffersForSearchType(searchRequest: ISearchRequestData, itemsToAdd: string[], traderAssorts: Record<string, ITraderAssort>, pmcProfile: IPmcData): IRagfairOffer[];
    /**
     * Get categories for the type of search being performed, linked/required/all
     * @param searchRequest Client search request data
     * @param offers ragfair offers to get categories for
     * @returns record with tpls + counts
     */
    protected getSpecificCategories(searchRequest: ISearchRequestData, offers: IRagfairOffer[]): Record<string, number>;
    /**
     * Add Required offers to offers result
     * @param searchRequest Client search request data
     * @param assorts
     * @param pmcProfile Player profile
     * @param result Result object being sent back to client
     */
    protected addRequiredOffersToResult(searchRequest: ISearchRequestData, assorts: Record<string, ITraderAssort>, pmcProfile: IPmcData, result: IGetOffersResult): void;
    /**
     * Add index to all offers passed in (0-indexed)
     * @param offers Offers to add index value to
     */
    protected addIndexValueToOffers(offers: IRagfairOffer[]): void;
    /**
     * Update a trader flea offer with buy restrictions stored in the traders assort
     * @param offer flea offer to update
     * @param profile full profile of player
     */
    protected setTraderOfferPurchaseLimits(offer: IRagfairOffer, profile: IAkiProfile): void;
    /**
     * Adjust ragfair offer stack count to match same value as traders assort stack count
     * @param offer Flea offer to adjust
     */
    protected setTraderOfferStackSize(offer: IRagfairOffer): void;
    protected isLinkedSearch(info: ISearchRequestData): boolean;
    protected isRequiredSearch(info: ISearchRequestData): boolean;
    update(): void;
    /**
     * Called when creating an offer on flea, fills values in top right corner
     * @param getPriceRequest
     * @returns min/avg/max values for an item based on flea offers available
     */
    getItemMinAvgMaxFleaPriceValues(getPriceRequest: IGetMarketPriceRequestData): IGetItemPriceResult;
    /**
     * List item(s) on flea for sale
     * @param pmcData Player profile
     * @param offerRequest Flea list creation offer
     * @param sessionID Session id
     * @returns IItemEventRouterResponse
     */
    addPlayerOffer(pmcData: IPmcData, offerRequest: IAddOfferRequestData, sessionID: string): IItemEventRouterResponse;
    /**
     * Charge player a listing fee for using flea, pulls charge from data previously sent by client
     * @param sessionID Player id
     * @param rootItem Base item being listed (used when client tax cost not found and must be done on server)
     * @param pmcData Player profile
     * @param requirementsPriceInRub Rouble cost player chose for listing (used when client tax cost not found and must be done on server)
     * @param itemStackCount How many items were listed in player (used when client tax cost not found and must be done on server)
     * @param offerRequest Add offer request object from client
     * @param output IItemEventRouterResponse
     * @returns True if charging tax to player failed
     */
    protected chargePlayerTaxFee(sessionID: string, rootItem: Item, pmcData: IPmcData, requirementsPriceInRub: number, itemStackCount: number, offerRequest: IAddOfferRequestData, output: IItemEventRouterResponse): boolean;
    /**
     * Is the item to be listed on the flea valid
     * @param offerRequest Client offer request
     * @param errorMessage message to show to player when offer is invalid
     * @returns Is offer valid
     */
    protected isValidPlayerOfferRequest(offerRequest: IAddOfferRequestData, errorMessage: string): boolean;
    /**
     * Get the handbook price in roubles for the items being listed
     * @param requirements
     * @returns Rouble price
     */
    protected calculateRequirementsPriceInRub(requirements: Requirement[]): number;
    /**
     * Using item ids from flea offer request, find corrispnding items from player inventory and return as array
     * @param pmcData Player profile
     * @param itemIdsFromFleaOfferRequest Ids from request
     * @param errorMessage if item is not found, add error message to this parameter
     * @returns Array of items from player inventory
     */
    protected getItemsToListOnFleaFromInventory(pmcData: IPmcData, itemIdsFromFleaOfferRequest: string[], errorMessage: string): Item[];
    createPlayerOffer(profile: IAkiProfile, requirements: Requirement[], items: Item[], sellInOnePiece: boolean, amountToSend: number): IRagfairOffer;
    getAllFleaPrices(): Record<string, number>;
    getStaticPrices(): Record<string, number>;
    /**
     * User requested removal of the offer, actually reduces the time to 71 seconds,
     * allowing for the possibility of extending the auction before it's end time
     * @param offerId offer to 'remove'
     * @param sessionID Players id
     * @returns IItemEventRouterResponse
     */
    removeOffer(offerId: string, sessionID: string): IItemEventRouterResponse;
    extendOffer(info: IExtendOfferRequestData, sessionID: string): IItemEventRouterResponse;
    /**
     * Create a basic trader request object with price and currency type
     * @param currency What currency: RUB, EURO, USD
     * @param value Amount of currency
     * @returns IProcessBuyTradeRequestData
     */
    protected createBuyTradeRequestObject(currency: string, value: number): IProcessBuyTradeRequestData;
}
