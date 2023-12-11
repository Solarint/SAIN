import { HandbookHelper } from "@spt-aki/helpers/HandbookHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { PresetHelper } from "@spt-aki/helpers/PresetHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { RagfairServerHelper } from "@spt-aki/helpers/RagfairServerHelper";
import { RepeatableQuestHelper } from "@spt-aki/helpers/RepeatableQuestHelper";
import { Exit } from "@spt-aki/models/eft/common/ILocationBase";
import { TraderInfo } from "@spt-aki/models/eft/common/tables/IBotBase";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { ICompletion, ICompletionAvailableFor, IElimination, IEliminationCondition, IExploration, IExplorationCondition, IPickup, IRepeatableQuest, IReward, IRewards } from "@spt-aki/models/eft/common/tables/IRepeatableQuests";
import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { IBaseQuestConfig, IBossInfo, IEliminationConfig, IQuestConfig, IRepeatableQuestConfig } from "@spt-aki/models/spt/config/IQuestConfig";
import { IQuestTypePool } from "@spt-aki/models/spt/repeatable/IQuestTypePool";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { EventOutputHolder } from "@spt-aki/routers/EventOutputHolder";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { ItemFilterService } from "@spt-aki/services/ItemFilterService";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { PaymentService } from "@spt-aki/services/PaymentService";
import { ProfileFixerService } from "@spt-aki/services/ProfileFixerService";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
import { MathUtil } from "@spt-aki/utils/MathUtil";
import { ObjectId } from "@spt-aki/utils/ObjectId";
import { ProbabilityObjectArray, RandomUtil } from "@spt-aki/utils/RandomUtil";
import { TimeUtil } from "@spt-aki/utils/TimeUtil";
export declare class RepeatableQuestGenerator {
    protected timeUtil: TimeUtil;
    protected logger: ILogger;
    protected randomUtil: RandomUtil;
    protected httpResponse: HttpResponseUtil;
    protected mathUtil: MathUtil;
    protected jsonUtil: JsonUtil;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected presetHelper: PresetHelper;
    protected profileHelper: ProfileHelper;
    protected profileFixerService: ProfileFixerService;
    protected handbookHelper: HandbookHelper;
    protected ragfairServerHelper: RagfairServerHelper;
    protected eventOutputHolder: EventOutputHolder;
    protected localisationService: LocalisationService;
    protected paymentService: PaymentService;
    protected objectId: ObjectId;
    protected itemFilterService: ItemFilterService;
    protected repeatableQuestHelper: RepeatableQuestHelper;
    protected configServer: ConfigServer;
    protected questConfig: IQuestConfig;
    constructor(timeUtil: TimeUtil, logger: ILogger, randomUtil: RandomUtil, httpResponse: HttpResponseUtil, mathUtil: MathUtil, jsonUtil: JsonUtil, databaseServer: DatabaseServer, itemHelper: ItemHelper, presetHelper: PresetHelper, profileHelper: ProfileHelper, profileFixerService: ProfileFixerService, handbookHelper: HandbookHelper, ragfairServerHelper: RagfairServerHelper, eventOutputHolder: EventOutputHolder, localisationService: LocalisationService, paymentService: PaymentService, objectId: ObjectId, itemFilterService: ItemFilterService, repeatableQuestHelper: RepeatableQuestHelper, configServer: ConfigServer);
    /**
     * This method is called by /GetClientRepeatableQuests/ and creates one element of quest type format (see assets/database/templates/repeatableQuests.json).
     * It randomly draws a quest type (currently Elimination, Completion or Exploration) as well as a trader who is providing the quest
     * @param pmcLevel Player's level for requested items and reward generation
     * @param pmcTraderInfo Players traper standing/rep levels
     * @param questTypePool Possible quest types pool
     * @param repeatableConfig Repeatable quest config
     * @returns IRepeatableQuest
     */
    generateRepeatableQuest(pmcLevel: number, pmcTraderInfo: Record<string, TraderInfo>, questTypePool: IQuestTypePool, repeatableConfig: IRepeatableQuestConfig): IRepeatableQuest;
    /**
     * Generate a randomised Elimination quest
     * @param pmcLevel Player's level for requested items and reward generation
     * @param traderId Trader from which the quest will be provided
     * @param questTypePool Pools for quests (used to avoid redundant quests)
     * @param repeatableConfig The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest
     * @returns Object of quest type format for "Elimination" (see assets/database/templates/repeatableQuests.json)
     */
    protected generateEliminationQuest(pmcLevel: number, traderId: string, questTypePool: IQuestTypePool, repeatableConfig: IRepeatableQuestConfig): IElimination;
    /**
     * Get a number of kills neded to complete elimination quest
     * @param targetKey Target type desired e.g. anyPmc/bossBully/Savage
     * @param targetsConfig Config
     * @param eliminationConfig Config
     * @returns Number of AI to kill
     */
    protected getEliminationKillCount(targetKey: string, targetsConfig: ProbabilityObjectArray<string, IBossInfo>, eliminationConfig: IEliminationConfig): number;
    /**
     * A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
     * This is a helper method for GenerateEliminationQuest to create a location condition.
     *
     * @param   {string}    location        the location on which to fulfill the elimination quest
     * @returns {IEliminationCondition}     object of "Elimination"-location-subcondition
     */
    protected generateEliminationLocation(location: string[]): IEliminationCondition;
    /**
     * Create kill condition for an elimination quest
     * @param target Bot type target of elimination quest e.g. "AnyPmc", "Savage"
     * @param targetedBodyParts Body parts player must hit
     * @param distance Distance from which to kill (currently only >= supported
     * @param allowedWeapon What weapon must be used - undefined = any
     * @param allowedWeaponCategory What category of weapon must be used - undefined = any
     * @returns IEliminationCondition object
     */
    protected generateEliminationCondition(target: string, targetedBodyParts: string[], distance: number, allowedWeapon: string, allowedWeaponCategory: string): IEliminationCondition;
    /**
     * Generates a valid Completion quest
     *
     * @param   {integer}   pmcLevel            player's level for requested items and reward generation
     * @param   {string}    traderId            trader from which the quest will be provided
     * @param   {object}    repeatableConfig    The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest
     * @returns {object}                        object of quest type format for "Completion" (see assets/database/templates/repeatableQuests.json)
     */
    protected generateCompletionQuest(pmcLevel: number, traderId: string, repeatableConfig: IRepeatableQuestConfig): ICompletion;
    /**
     * A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
     * This is a helper method for GenerateCompletionQuest to create a completion condition (of which a completion quest theoretically can have many)
     *
     * @param   {string}    itemTpl    id of the item to request
     * @param   {integer}   value           amount of items of this specific type to request
     * @returns {object}                    object of "Completion"-condition
     */
    protected generateCompletionAvailableForFinish(itemTpl: string, value: number): ICompletionAvailableFor;
    /**
     * Generates a valid Exploration quest
     *
     * @param   {integer}   pmcLevel            player's level for reward generation
     * @param   {string}    traderId            trader from which the quest will be provided
     * @param   {object}    questTypePool       Pools for quests (used to avoid redundant quests)
     * @param   {object}    repeatableConfig    The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest
     * @returns {object}                        object of quest type format for "Exploration" (see assets/database/templates/repeatableQuests.json)
     */
    protected generateExplorationQuest(pmcLevel: number, traderId: string, questTypePool: IQuestTypePool, repeatableConfig: IRepeatableQuestConfig): IExploration;
    protected generatePickupQuest(pmcLevel: number, traderId: string, questTypePool: IQuestTypePool, repeatableConfig: IRepeatableQuestConfig): IPickup;
    /**
     * Convert a location into an quest code can read (e.g. factory4_day into 55f2d3fd4bdc2d5f408b4567)
     * @param locationKey e.g factory4_day
     * @returns guid
     */
    protected getQuestLocationByMapId(locationKey: string): string;
    /**
     * Exploration repeatable quests can specify a required extraction point.
     * This method creates the according object which will be appended to the conditions array
     *
     * @param   {string}        exit                The exit name to generate the condition for
     * @returns {object}                            Exit condition
     */
    protected generateExplorationExitCondition(exit: Exit): IExplorationCondition;
    /**
     * Generate the reward for a mission. A reward can consist of
     * - Experience
     * - Money
     * - Items
     * - Trader Reputation
     *
     * The reward is dependent on the player level as given by the wiki. The exact mapping of pmcLevel to
     * experience / money / items / trader reputation can be defined in QuestConfig.js
     *
     * There's also a random variation of the reward the spread of which can be also defined in the config.
     *
     * Additonaly, a scaling factor w.r.t. quest difficulty going from 0.2...1 can be used
     *
     * @param   {integer}   pmcLevel            player's level
     * @param   {number}    difficulty          a reward scaling factor goint from 0.2 to 1
     * @param   {string}    traderId            the trader for reputation gain (and possible in the future filtering of reward item type based on trader)
     * @param   {object}    repeatableConfig    The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest
     * @returns {object}                        object of "Reward"-type that can be given for a repeatable mission
     */
    protected generateReward(pmcLevel: number, difficulty: number, traderId: string, repeatableConfig: IRepeatableQuestConfig, questConfig: IBaseQuestConfig): IRewards;
    /**
     * Should reward item have stack size increased (25% chance)
     * @param item Item to possibly increase stack size of
     * @param maxRoublePriceToStack Maximum rouble price an item can be to still be chosen for stacking
     * @returns True if it should
     */
    protected canIncreaseRewardItemStackSize(item: ITemplateItem, maxRoublePriceToStack: number): boolean;
    /**
     * Get a randomised number a reward items stack size should be based on its handbook price
     * @param item Reward item to get stack size for
     * @returns Stack size value
     */
    protected getRandomisedRewardItemStackSizeByPrice(item: ITemplateItem): number;
    /**
     * Select a number of items that have a colelctive value of the passed in parameter
     * @param repeatableConfig Config
     * @param roublesBudget Total value of items to return
     * @returns Array of reward items that fit budget
     */
    protected chooseRewardItemsWithinBudget(repeatableConfig: IRepeatableQuestConfig, roublesBudget: number, traderId: string): ITemplateItem[];
    /**
     * Helper to create a reward item structured as required by the client
     *
     * @param   {string}    tpl             ItemId of the rewarded item
     * @param   {integer}   value           Amount of items to give
     * @param   {integer}   index           All rewards will be appended to a list, for unknown reasons the client wants the index
     * @returns {object}                    Object of "Reward"-item-type
     */
    protected generateRewardItem(tpl: string, value: number, index: number, preset?: Item[]): IReward;
    /**
     * Picks rewardable items from items.json. This means they need to fit into the inventory and they shouldn't be keys (debatable)
     * @param repeatableQuestConfig Config file
     * @returns List of rewardable items [[_tpl, itemTemplate],...]
     */
    protected getRewardableItems(repeatableQuestConfig: IRepeatableQuestConfig, traderId: string): [string, ITemplateItem][];
    /**
     * Checks if an id is a valid item. Valid meaning that it's an item that may be a reward
     * or content of bot loot. Items that are tested as valid may be in a player backpack or stash.
     * @param {string} tpl template id of item to check
     * @returns True if item is valid reward
     */
    protected isValidRewardItem(tpl: string, repeatableQuestConfig: IRepeatableQuestConfig, itemBaseWhitelist: string[]): boolean;
    /**
     * Generates the base object of quest type format given as templates in assets/database/templates/repeatableQuests.json
     * The templates include Elimination, Completion and Extraction quest types
     *
     * @param   {string}    type            Quest type: "Elimination", "Completion" or "Extraction"
     * @param   {string}    traderId        Trader from which the quest will be provided
     * @param   {string}    side            Scav daily or pmc daily/weekly quest
     * @returns {object}                    Object which contains the base elements for repeatable quests of the requests type
     *                                      (needs to be filled with reward and conditions by called to make a valid quest)
     */
    protected generateRepeatableTemplate(type: string, traderId: string, side: string): IRepeatableQuest;
}
