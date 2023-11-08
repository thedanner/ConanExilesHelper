namespace ConanExilesHelper.SourceQuery.Rules;

public class ConanExilesRules
{
    public static readonly IRuleParser<ConanExilesRules> Parser = new AttributeBasedRuleParser<ConanExilesRules>();

    public const string KeyNamePVPEnabled = "S0";
    [FromKey(KeyNamePVPEnabled)]
    public string? PvpEnabled { get; set; }

    public const string KeyNameNoOwnership = "S1";
    [FromKey(KeyNameNoOwnership)]
    public string? NoOwnership { get; set; }

    public const string KeyNameCanDamagePlayerOwnedStructures = "S2";
    [FromKey(KeyNameCanDamagePlayerOwnedStructures)]
    public string? CanDamagePlayerOwnedStructures { get; set; }

    public const string KeyNameEnableSandStorm = "S3";
    [FromKey(KeyNameEnableSandStorm)]
    public string? EnableSandStorm { get; set; }

    public const string KeyNameThrallConversionMultiplier = "S4";
    [FromKey(KeyNameThrallConversionMultiplier)]
    public string? ThrallConversionMultiplier { get; set; }

    public const string KeyNameLogoutCharactersRemainInTheWorld = "S5";
    [FromKey(KeyNameLogoutCharactersRemainInTheWorld)]
    public string? LogoutCharactersRemainInTheWorld { get; set; }

    public const string KeyNameDurabilityMultiplier = "S6";
    [FromKey(KeyNameDurabilityMultiplier)]
    public string? DurabilityMultiplier { get; set; }

    public const string KeyNameDropEquipmentOnDeath = "S7";
    [FromKey(KeyNameDropEquipmentOnDeath)]
    public string? DropEquipmentOnDeath { get; set; }

    public const string KeyNameItemConvertionMultiplier = "S8";
    [FromKey(KeyNameItemConvertionMultiplier)]
    public string? ItemConvertionMultiplier { get; set; }

    public const string KeyNameEverybodyCanLootCorpse = "Sa";
    [FromKey(KeyNameEverybodyCanLootCorpse)]
    public string? EverybodyCanLootCorpse { get; set; }

    public const string KeyNameDayCycleSpeedScale = "Sb";
    [FromKey(KeyNameDayCycleSpeedScale)]
    public string? DayCycleSpeedScale { get; set; }

    public const string KeyNameClientCatchUpTime = "Sc";
    [FromKey(KeyNameClientCatchUpTime)]
    public string? ClientCatchUpTime { get; set; }

    public const string KeyNameUseClientCatchUpTime = "Sd";
    [FromKey(KeyNameUseClientCatchUpTime)]
    public string? UseClientCatchUpTime { get; set; }

    public const string KeyNameDawnDuskSpeedScale = "Sg";
    [FromKey(KeyNameDawnDuskSpeedScale)]
    public string? DawnDuskSpeedScale { get; set; }

    public const string KeyNamePlayerHealthMultiplier = "Sh";
    [FromKey(KeyNamePlayerHealthMultiplier)]
    public string? PlayerHealthMultiplier { get; set; }

    public const string KeyNamePlayerStaminaMultiplier = "Si";
    [FromKey(KeyNamePlayerStaminaMultiplier)]
    public string? PlayerStaminaMultiplier { get; set; }

    public const string KeyNameStaminaCostMultiplier = "Sj";
    [FromKey(KeyNameStaminaCostMultiplier)]
    public string? StaminaCostMultiplier { get; set; }

    public const string KeyNameItemSpoilRateScale = "Sk";
    [FromKey(KeyNameItemSpoilRateScale)]
    public string? ItemSpoilRateScale { get; set; }

    public const string KeyNameHarvestAmountMultiplier = "Sl";
    [FromKey(KeyNameHarvestAmountMultiplier)]
    public string? HarvestAmountMultiplier { get; set; }

    public const string KeyNameResourceRespawnSpeedMultiplier = "Sm";
    [FromKey(KeyNameResourceRespawnSpeedMultiplier)]
    public string? ResourceRespawnSpeedMultiplier { get; set; }

    public const string KeyNameNPCMindReadingMode = "Sn";
    [FromKey(KeyNameNPCMindReadingMode)]
    public string? NPCMindReadingMode { get; set; }

    public const string KeyNameUnconsciousTimeSeconds = "So";
    [FromKey(KeyNameUnconsciousTimeSeconds)]
    public string? UnconsciousTimeSeconds { get; set; }

    public const string KeyNameMaxNudity = "Sp";
    [FromKey(KeyNameMaxNudity)]
    public string? MaxNudity { get; set; }

    public const string KeyNameChatHasGlobal = "Sq";
    [FromKey(KeyNameChatHasGlobal)]
    public string? ChatHasGlobal { get; set; }

    public const string KeyNameChatLocalRadius = "Sr";
    [FromKey(KeyNameChatLocalRadius)]
    public string? ChatLocalRadius { get; set; }

    public const string KeyNameChatMaxMessageLength = "Ss";
    [FromKey(KeyNameChatMaxMessageLength)]
    public string? ChatMaxMessageLength { get; set; }

    public const string KeyNameChatFloodControlAheadCounter = "St";
    [FromKey(KeyNameChatFloodControlAheadCounter)]
    public string? ChatFloodControlAheadCounter { get; set; }

    public const string KeyNameServerCommunity = "Su";
    [FromKey(KeyNameServerCommunity)]
    public string? ServerCommunity { get; set; }

    public const string KeyNameAvatarSummonTime = "Sv";
    [FromKey(KeyNameAvatarSummonTime)]
    public string? AvatarSummonTime { get; set; }

    public const string KeyNameAvatarLifetime = "Sw";
    [FromKey(KeyNameAvatarLifetime)]
    public string? AvatarLifetime { get; set; }

    public const string KeyNameClanMaxSize = "Sx";
    [FromKey(KeyNameClanMaxSize)]
    public string? ClanMaxSize { get; set; }

    public const string KeyNameServerRegion = "Sy";
    [FromKey(KeyNameServerRegion)]
    public string? ServerRegion { get; set; }

    public const string KeyNamePlayerXPRateMultiplier = "Sz";
    [FromKey(KeyNamePlayerXPRateMultiplier)]
    public string? PlayerXPRateMultiplier { get; set; }

    public const string KeyNamePlayerXPKillMultiplier = "S00";
    [FromKey(KeyNamePlayerXPKillMultiplier)]
    public string? PlayerXPKillMultiplier { get; set; }

    public const string KeyNamePlayerXPHarvestMultiplier = "S01";
    [FromKey(KeyNamePlayerXPHarvestMultiplier)]
    public string? PlayerXPHarvestMultiplier { get; set; }

    public const string KeyNamePlayerXPCraftMultiplier = "S02";
    [FromKey(KeyNamePlayerXPCraftMultiplier)]
    public string? PlayerXPCraftMultiplier { get; set; }

    public const string KeyNamePlayerXPTimeMultiplier = "S03";
    [FromKey(KeyNamePlayerXPTimeMultiplier)]
    public string? PlayerXPTimeMultiplier { get; set; }

    public const string KeyNameLandClaimRadiusMultiplier = "S04";
    [FromKey(KeyNameLandClaimRadiusMultiplier)]
    public string? LandClaimRadiusMultiplier { get; set; }

    public const string KeyNameIsBattlEyeEnabled = "S05";
    [FromKey(KeyNameIsBattlEyeEnabled)]
    public string? IsBattlEyeEnabled { get; set; }

    public const string KeyNameRegionAllowAfrica = "S06";
    [FromKey(KeyNameRegionAllowAfrica)]
    public string? RegionAllowAfrica { get; set; }

    public const string KeyNameRegionAllowAsia = "S07";
    [FromKey(KeyNameRegionAllowAsia)]
    public string? RegionAllowAsia { get; set; }

    public const string KeyNameRegionAllowCentralEurope = "S08";
    [FromKey(KeyNameRegionAllowCentralEurope)]
    public string? RegionAllowCentralEurope { get; set; }

    public const string KeyNameRegionAllowEasternEurope = "S09";
    [FromKey(KeyNameRegionAllowEasternEurope)]
    public string? RegionAllowEasternEurope { get; set; }

    public const string KeyNameRegionAllowWesternEurope = "S10";
    [FromKey(KeyNameRegionAllowWesternEurope)]
    public string? RegionAllowWesternEurope { get; set; }

    public const string KeyNameRegionAllowNorthAmerica = "S11";
    [FromKey(KeyNameRegionAllowNorthAmerica)]
    public string? RegionAllowNorthAmerica { get; set; }

    public const string KeyNameRegionAllowOceania = "S12";
    [FromKey(KeyNameRegionAllowOceania)]
    public string? RegionAllowOceania { get; set; }

    public const string KeyNameRegionAllowSouthAmerica = "S13";
    [FromKey(KeyNameRegionAllowSouthAmerica)]
    public string? RegionAllowSouthAmerica { get; set; }

    public const string KeyNameRegionBlockList = "S14";
    [FromKey(KeyNameRegionBlockList)]
    public string? RegionBlockList { get; set; }

    public const string KeyNameServerVoiceChat = "S16";
    [FromKey(KeyNameServerVoiceChat)]
    public string? ServerVoiceChat { get; set; }

    public const string KeyNameServerModList = "S17";
    [FromKey(KeyNameServerModList)]
    public string? ServerModList { get; set; }

    public const string KeyNameIsVACEnabled = "S18";
    [FromKey(KeyNameIsVACEnabled)]
    public string? IsVACEnabled { get; set; }

    public const string KeyNameIsLoadErrorsFatal = "S19";
    [FromKey(KeyNameIsLoadErrorsFatal)]
    public string? IsLoadErrorsFatal { get; set; }

    public const string KeyNameMaxAllowedPing = "S20";
    [FromKey(KeyNameMaxAllowedPing)]
    public string? MaxAllowedPing { get; set; }

    public const string KeyNamePlayerIdleThirstMultiplier = "S21";
    [FromKey(KeyNamePlayerIdleThirstMultiplier)]
    public string? PlayerIdleThirstMultiplier { get; set; }

    public const string KeyNamePlayerActiveThirstMultiplier = "S22";
    [FromKey(KeyNamePlayerActiveThirstMultiplier)]
    public string? PlayerActiveThirstMultiplier { get; set; }

    public const string KeyNamePlayerIdleHungerMultiplier = "S23";
    [FromKey(KeyNamePlayerIdleHungerMultiplier)]
    public string? PlayerIdleHungerMultiplier { get; set; }

    public const string KeyNamePlayerActiveHungerMultiplier = "S24";
    [FromKey(KeyNamePlayerActiveHungerMultiplier)]
    public string? PlayerActiveHungerMultiplier { get; set; }

    public const string KeyNameRestrictPVPBuildingDamageTime = "S25";
    [FromKey(KeyNameRestrictPVPBuildingDamageTime)]
    public string? RestrictPVPBuildingDamageTime { get; set; }

    public const string KeyNamePVPBuildingDamageTimeWeekdayStart = "S26";
    [FromKey(KeyNamePVPBuildingDamageTimeWeekdayStart)]
    public string? PVPBuildingDamageTimeWeekdayStart { get; set; }

    public const string KeyNamePVPBuildingDamageTimeWeekdayEnd = "S27";
    [FromKey(KeyNamePVPBuildingDamageTimeWeekdayEnd)]
    public string? PVPBuildingDamageTimeWeekdayEnd { get; set; }

    public const string KeyNamePVPBuildingDamageTimeWeekendStart = "S28";
    [FromKey(KeyNamePVPBuildingDamageTimeWeekendStart)]
    public string? PVPBuildingDamageTimeWeekendStart { get; set; }

    public const string KeyNamePVPBuildingDamageTimeWeekendEnd = "S29";
    [FromKey(KeyNamePVPBuildingDamageTimeWeekendEnd)]
    public string? PVPBuildingDamageTimeWeekendEnd { get; set; }

    public const string KeyNameCombatModeModifier = "S30";
    [FromKey(KeyNameCombatModeModifier)]
    public string? CombatModeModifier { get; set; }

    public const string KeyNameBuildingPreloadRadius = "S31";
    [FromKey(KeyNameBuildingPreloadRadius)]
    public string? BuildingPreloadRadius { get; set; }

    public const string KeyNameCoopTetheringDistance = "S32";
    [FromKey(KeyNameCoopTetheringDistance)]
    public string? CoopTetheringDistance { get; set; }

    public const string KeyNamePurgeLevel = "S33";
    [FromKey(KeyNamePurgeLevel)]
    public string? PurgeLevel { get; set; }

    public const string KeyNamePurgePeriodicity = "S34";
    [FromKey(KeyNamePurgePeriodicity)]
    public string? PurgePeriodicity { get; set; }

    public const string KeyNameRestrictPurgeTime = "S35";
    [FromKey(KeyNameRestrictPurgeTime)]
    public string? RestrictPurgeTime { get; set; }

    public const string KeyNamePurgeRestrictionWeekdayStart = "S36";
    [FromKey(KeyNamePurgeRestrictionWeekdayStart)]
    public string? PurgeRestrictionWeekdayStart { get; set; }

    public const string KeyNamePurgeRestrictionWeekdayEnd = "S37";
    [FromKey(KeyNamePurgeRestrictionWeekdayEnd)]
    public string? PurgeRestrictionWeekdayEnd { get; set; }

    public const string KeyNamePurgeRestrictionWeekendStart = "S38";
    [FromKey(KeyNamePurgeRestrictionWeekendStart)]
    public string? PurgeRestrictionWeekendStart { get; set; }

    public const string KeyNamePPurgeRestrictionWeekendEnd = "S39";
    [FromKey(KeyNamePPurgeRestrictionWeekendEnd)]
    public string? PPurgeRestrictionWeekendEnd { get; set; }

    public const string KeyNamePurgePreparationTime = "S40";
    [FromKey(KeyNamePurgePreparationTime)]
    public string? PurgePreparationTime { get; set; }

    public const string KeyNamePurgeDuration = "S41";
    [FromKey(KeyNamePurgeDuration)]
    public string? PurgeDuration { get; set; }

    public const string KeyNameMinPurgeOnlinePlayers = "S42";
    [FromKey(KeyNameMinPurgeOnlinePlayers)]
    public string? MinPurgeOnlinePlayers { get; set; }

    public const string KeyNameAllowBuilding = "S43";
    [FromKey(KeyNameAllowBuilding)]
    public string? AllowBuilding { get; set; }

    public const string KeyNameClanPurgeTrigger = "S44";
    [FromKey(KeyNameClanPurgeTrigger)]
    public string? ClanPurgeTrigger { get; set; }

    public const string KeyNameClanScoreUpateFrequency = "S45";
    [FromKey(KeyNameClanScoreUpateFrequency)]
    public string? ClanScoreUpateFrequency { get; set; }

    public const string KeyNameEnablePurge = "S46";
    [FromKey(KeyNameEnablePurge)]
    public string? EnablePurge { get; set; }

    public const string KeyNamePurgeNPCBuildingDamageMultiplier = "S47";
    [FromKey(KeyNamePurgeNPCBuildingDamageMultiplier)]
    public string? PurgeNPCBuildingDamageMultiplier { get; set; }
}
