/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/brace-style */

import { ConfigTypes } from "@spt-aki/models/enums/ConfigTypes";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";
import { IPmcConfig } from "@spt-aki/models/spt/config/IPmcConfig";
import { IBotConfig } from "@spt-aki/models/spt/config/IBotConfig";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { DependencyContainer } from "tsyringe";

let botConfig: IBotConfig;
let pmcConfig: IPmcConfig;
let configServer: ConfigServer;

class SAIN implements IPostDBLoadMod {
    public postDBLoad(container: DependencyContainer): void {
        configServer = container.resolve<ConfigServer>("ConfigServer");
        pmcConfig = configServer.getConfig<IPmcConfig>(ConfigTypes.PMC);
        botConfig = configServer.getConfig<IBotConfig>(ConfigTypes.BOT);
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();

        // Only allow `pmcBot` brains to spawn for PMCs
        for (const pmcType in pmcConfig.pmcType)
        {
            for (const map in pmcConfig.pmcType[pmcType])
            {
                const pmcBrains = pmcConfig.pmcType[pmcType][map]
                for (const brain in pmcBrains)
                {
                    if (brain === "pmcBot")
                    {
                        pmcBrains[brain] = 1;
                    }
                    else
                    {
                        pmcBrains[brain] = 0;
                    }
                }
            }
        }

        // Only allow `assault` brains for scavs
        for (const map in botConfig.assaultBrainType)
        {
            const scavBrains = botConfig.assaultBrainType[map];
            for (const brain in scavBrains)
            {
                if (brain === "assault")
                {
                    scavBrains[brain] = 1;
                }
                else
                {
                    scavBrains[brain] = 0;
                }
            }
        }

        // Only allow `pmcBot` brains for player scavs
        for (const map in botConfig.playerScavBrainType)
        {
            const playerScavBrains = botConfig.playerScavBrainType[map];
            for (const brain in playerScavBrains)
            {
                if (brain === "pmcBot")
                {
                    playerScavBrains[brain] = 1;
                }
                else
                {
                    playerScavBrains[brain] = 0;
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