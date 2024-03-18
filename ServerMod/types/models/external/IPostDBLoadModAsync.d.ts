import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPostDBLoadModAsync {
    postDBLoadAsync(container: DependencyContainer): Promise<void>;
}
