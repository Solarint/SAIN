import { Item } from "@spt-aki/models/eft/common/tables/IItem";
export interface IReward {
    index: number;
    type: string;
    value: number;
    target?: string;
    items?: Item[];
}
export interface IRepeatableQuestDatabase {
    templates: ITemplates;
    rewards: IRewardOptions;
    data: IOptions;
    samples: ISampleQuests[];
}
export interface ITemplates {
    Elimination: IRepeatableQuest;
    Completion: IRepeatableQuest;
    Exploration: IRepeatableQuest;
}
export interface IPmcDataRepeatableQuest {
    id?: string;
    name: string;
    activeQuests: IRepeatableQuest[];
    inactiveQuests: IRepeatableQuest[];
    endTime: number;
    changeRequirement: TChangeRequirementRecord;
}
export type TChangeRequirementRecord = Record<string, IChangeRequirement>;
export interface IChangeRequirement {
    changeCost: IChangeCost[];
    changeStandingCost: number;
}
export interface IChangeCost {
    templateId: string;
    count: number;
}
export interface IRepeatableQuest {
    _id: string;
    traderId: string;
    location: string;
    image: string;
    type: string;
    isKey: boolean;
    restartable: boolean;
    instantComplete: boolean;
    secretQuest: boolean;
    canShowNotificationsInGame: boolean;
    rewards: IRewards;
    conditions: IConditions;
    side: string;
    questStatus: any;
    name: string;
    note: string;
    description: string;
    successMessageText: string;
    failMessageText: string;
    startedMessageText: string;
    changeQuestMessageText: string;
    acceptPlayerMessage: string;
    declinePlayerMessage: string;
    completePlayerMessage: string;
    templateId: string;
    changeCost: IChangeCost[];
    changeStandingCost: number;
    sptRepatableGroupName?: string;
}
export interface IRewards {
    Started: IReward[];
    Success: IReward[];
    Fail: IReward[];
}
export interface IConditions {
    AvailableForStart: any[];
    AvailableForFinish: IAvailableFor[];
    Fail: any[];
}
export interface IAvailableFor {
    _props: IAvailableForProps;
    _parent: string;
    dynamicLocale: boolean;
}
export interface IAvailableForProps {
    id: string;
    parentId: string;
    dynamicLocale: boolean;
    index: number;
    visibilityConditions: IVisibilityCondition[];
    value: number;
}
export interface IVisibilityCondition {
    id: string;
    oneSessionOnly: boolean;
    value: number;
    index: number;
    dynamicLocale: boolean;
}
export interface IAvailableForPropsCounter extends IAvailableForProps {
    type: string;
    oneSessionOnly: boolean;
    doNotResetIfCounterCompleted: boolean;
    counter?: ICounter;
}
export interface ICounter {
    id: string;
    conditions: ICondition[];
}
export interface ICondition {
    _props: IConditionProps;
    _parent: string;
}
export interface IConditionProps {
    id: string;
    dynamicLocale: boolean;
}
export interface IElimination extends IRepeatableQuest {
    conditions: IEliminationConditions;
}
export interface IEliminationConditions extends IConditions {
    AvailableForFinish: IEliminationAvailableFor[];
}
export interface IEliminationAvailableFor extends IAvailableFor {
    _props: IEliminationAvailableForProps;
}
export interface IEliminationAvailableForProps extends IAvailableForPropsCounter {
    counter: IEliminationCounter;
}
export interface IEliminationCounter extends ICounter {
    conditions: IEliminationCondition[];
}
export interface IEliminationCondition extends ICondition {
    _props: ILocationConditionProps | IKillConditionProps;
}
export interface IExploration extends IRepeatableQuest {
    conditions: IExplorationConditions;
}
export interface IExplorationConditions extends IConditions {
    AvailableForFinish: IExplorationAvailableFor[];
}
export interface IExplorationAvailableFor extends IAvailableFor {
    _props: IExplorationAvailableForProps;
}
export interface IExplorationAvailableForProps extends IAvailableForPropsCounter {
    counter: IExplorationCounter;
}
export interface IExplorationCounter extends ICounter {
    conditions: IExplorationCondition[];
}
export interface IExplorationCondition extends ICondition {
    _props: ILocationConditionProps | IExitStatusConditionProps | IExitNameConditionProps;
}
export interface IPickup extends IRepeatableQuest {
    conditions: IPickupConditions;
}
export interface IPickupConditions extends IConditions {
    AvailableForFinish: IPickupAvailableFor[];
}
export interface IPickupAvailableFor extends IAvailableFor {
    _props: IPickupAvailableForProps;
}
export interface IPickupAvailableForProps extends IAvailableForPropsCounter {
    target: string[];
    counter?: IPickupCounter;
}
export interface IPickupCounter extends ICounter {
    conditions: IPickupCondition[];
}
export interface IPickupCondition extends ICondition {
    _props: IEquipmentConditionProps | ILocationConditionProps | IExitStatusConditionProps;
}
export interface ICompletion extends IRepeatableQuest {
    conditions: ICompletionConditions;
}
export interface ICompletionConditions extends IConditions {
    AvailableForFinish: ICompletionAvailableFor[];
}
export interface ICompletionAvailableFor extends IAvailableFor {
    _props: ICompletionAvailableForProps;
}
export interface ICompletionAvailableForProps extends IAvailableForProps {
    target: string[];
    minDurability: number;
    maxDurability: number;
    dogtagLevel: number;
    onlyFoundInRaid: boolean;
}
export interface ILocationConditionProps extends IConditionProps {
    target: string[];
    weapon?: string[];
    weaponCategories?: string[];
}
export interface IEquipmentConditionProps extends IConditionProps {
    equipmentInclusive: [string[]];
    IncludeNotEquippedItems: boolean;
}
export interface IKillConditionProps extends IConditionProps {
    target: string;
    value: number;
    savageRole?: string[];
    bodyPart?: string[];
    distance?: IDistanceCheck;
    weapon?: string[];
    weaponCategories?: string[];
}
export interface IDistanceCheck {
    compareMethod: string;
    value: number;
}
export interface IExitStatusConditionProps extends IConditionProps {
    status: string[];
}
export interface IExitNameConditionProps extends IConditionProps {
    exitName: string;
}
export interface IRewardOptions {
    itemsBlacklist: string[];
}
export interface IOptions {
    Completion: ICompletionFilter;
}
export interface ICompletionFilter {
    itemsBlacklist: ItemsBlacklist[];
    itemsWhitelist: ItemsWhitelist[];
}
export interface ItemsBlacklist {
    minPlayerLevel: number;
    itemIds: string[];
}
export interface ItemsWhitelist {
    minPlayerLevel: number;
    itemIds: string[];
}
export interface ISampleQuests {
    _id: string;
    traderId: string;
    location: string;
    image: string;
    type: string;
    isKey: boolean;
    restartable: boolean;
    instantComplete: boolean;
    secretQuest: boolean;
    canShowNotificationsInGame: boolean;
    rewards: IRewards;
    conditions: IConditions;
    name: string;
    note: string;
    description: string;
    successMessageText: string;
    failMessageText: string;
    startedMessageText: string;
    templateId: string;
}
