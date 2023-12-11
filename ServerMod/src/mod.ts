/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/brace-style */

import { ConfigTypes } from "@spt-aki/models/enums/ConfigTypes";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";
import { IPmcConfig } from "@spt-aki/models/spt/config/IPmcConfig";
import { IBotConfig } from "@spt-aki/models/spt/config/IBotConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { DependencyContainer } from "tsyringe";

let botConfig: IBotConfig;
let pmcConfig: IPmcConfig;
let configServer: ConfigServer;
let logger: ILogger;

class SAIN implements IPostDBLoadMod {
    public postDBLoad(container: DependencyContainer): void {
        logger = container.resolve<ILogger>("WinstonLogger");
        configServer = container.resolve<ConfigServer>("ConfigServer");
        pmcConfig = configServer.getConfig<IPmcConfig>(ConfigTypes.PMC);
        botConfig = configServer.getConfig<IBotConfig>(ConfigTypes.BOT);
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();

        // Get all PMCTypes dynamically
        const pmcTypes = Object.keys(pmcConfig.pmcType);

        // Loop through each PMCTypes
        for (const pmcType of pmcTypes) {
            const maps = Object.keys(pmcConfig.pmcType[pmcType]);

            // Loop through each map
            for (const map of maps) {
                const brain = pmcConfig.pmcType[pmcType][map];

                for (const prop in brain)
                {
                    if (prop === 'pmcBot')
                    {
                        brain[prop] = 1;  // Set pmcBot property to 1
                    }
                    else
                    {
                        brain[prop] = 0;  // Set all other properties to 0
                    }
                }
            }
        }

        const maps = Object.keys((botConfig.assaultBrainType));

        // Loop through each map
        for (const map of maps) {
            const brain = botConfig.assaultBrainType[map];

            for (const prop in brain)
            {
                if (prop === 'assault')
                {
                    brain[prop] = 1;  // Set assault property to 1
                }
                else
                {
                    brain[prop] = 0;  // Set all other properties to 0
                }
            }
        }

        for (const locationName in tables.locations)
        {
            const location = tables.locations[locationName].base;

            if (location && location.BotLocationModifier)
            {
                location.BotLocationModifier.AccuracySpeed = 1;
                location.BotLocationModifier.GainSight = 1;
                location.BotLocationModifier.Scattering = 1;
                location.BotLocationModifier.VisibleDistance = 1;
            }
        }
    }
}
module.exports = { mod: new SAIN() }