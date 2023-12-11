import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { TradeHelper } from "@spt-aki/helpers/TradeHelper";
import { TraderHelper } from "@spt-aki/helpers/TraderHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { Item, Upd } from "@spt-aki/models/eft/common/tables/IItem";
import { ITraderBase } from "@spt-aki/models/eft/common/tables/ITrader";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { IProcessBaseTradeRequestData } from "@spt-aki/models/eft/trade/IProcessBaseTradeRequestData";
import { IProcessRagfairTradeRequestData } from "@spt-aki/models/eft/trade/IProcessRagfairTradeRequestData";
import { ISellScavItemsToFenceRequestData } from "@spt-aki/models/eft/trade/ISellScavItemsToFenceRequestData";
import { Traders } from "@spt-aki/models/enums/Traders";
import { IRagfairConfig } from "@spt-aki/models/spt/config/IRagfairConfig";
import { ITraderConfig } from "@spt-aki/models/spt/config/ITraderConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { EventOutputHolder } from "@spt-aki/routers/EventOutputHolder";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { RagfairServer } from "@spt-aki/servers/RagfairServer";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { RagfairPriceService } from "@spt-aki/services/RagfairPriceService";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
declare class TradeController {
    protected logger: ILogger;
    protected eventOutputHolder: EventOutputHolder;
    protected tradeHelper: TradeHelper;
    protected itemHelper: ItemHelper;
    protected profileHelper: ProfileHelper;
    protected traderHelper: TraderHelper;
    protected jsonUtil: JsonUtil;
    protected ragfairServer: RagfairServer;
    protected httpResponse: HttpResponseUtil;
    protected localisationService: LocalisationService;
    protected ragfairPriceService: RagfairPriceService;
    protected configServer: ConfigServer;
    protected ragfairConfig: IRagfairConfig;
    protected traderConfig: ITraderConfig;
    constructor(logger: ILogger, eventOutputHolder: EventOutputHolder, tradeHelper: TradeHelper, itemHelper: ItemHelper, profileHelper: ProfileHelper, traderHelper: TraderHelper, jsonUtil: JsonUtil, ragfairServer: RagfairServer, httpResponse: HttpResponseUtil, localisationService: LocalisationService, ragfairPriceService: RagfairPriceService, configServer: ConfigServer);
    /** Handle TradingConfirm event */
    confirmTrading(pmcData: IPmcData, request: IProcessBaseTradeRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle RagFairBuyOffer event */
    confirmRagfairTrading(pmcData: IPmcData, body: IProcessRagfairTradeRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle SellAllFromSavage event */
    sellScavItemsToFence(pmcData: IPmcData, request: ISellScavItemsToFenceRequestData, sessionId: string): IItemEventRouterResponse;
    /**
     * Sell all sellable items to a trader from inventory
     * WILL DELETE ITEMS FROM INVENTORY + CHILDREN OF ITEMS SOLD
     * @param sessionId Session id
     * @param profileWithItemsToSell Profile with items to be sold to trader
     * @param profileThatGetsMoney Profile that gets the money after selling items
     * @param trader Trader to sell items to
     * @returns IItemEventRouterResponse
     */
    protected sellInventoryToTrader(sessionId: string, profileWithItemsToSell: IPmcData, profileThatGetsMoney: IPmcData, trader: Traders): IItemEventRouterResponse;
    /**
     * Looks up an items children and gets total handbook price for them
     * @param parentItemId parent item that has children we want to sum price of
     * @param items All items (parent + children)
     * @param handbookPrices Prices of items from handbook
     * @param traderDetails Trader being sold to to perform buy category check against
     * @returns Rouble price
     */
    protected getPriceOfItemAndChildren(parentItemId: string, items: Item[], handbookPrices: Record<string, number>, traderDetails: ITraderBase): number;
    protected confirmTradingInternal(pmcData: IPmcData, body: IProcessBaseTradeRequestData, sessionID: string, foundInRaid?: boolean, upd?: Upd): IItemEventRouterResponse;
}
export { TradeController };
