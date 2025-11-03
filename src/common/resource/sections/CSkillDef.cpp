#include "../../../sphere/threads.h"
//#include "../../CException.h" // included in the precompiled header
#include "../../CScript.h"
#include "CSkillDef.h"

lpctstr const CSkillDef::sm_szTrigName[SKTRIG_QTY+1] =
{
    "@AAAUNUSED",
    "@ABORT",
    "@FAIL",	// we failed the skill check.
    "@GAIN",
    "@PRESTART",
    "@SELECT",	// just selecting params for the skill
    "@START",	// params for skill are done.
    "@STROKE",
    "@SUCCESS",
    "@TARGETCANCEL",
    "@USEQUICK",
    "@WAIT",
    nullptr
};

enum SKC_TYPE
{
    SKC_ADV_RATE,
    SKC_BONUS_DEX,
    SKC_BONUS_INT,
    SKC_BONUS_STATS,
    SKC_BONUS_STR,
    SKC_DEFNAME,
    SKC_DELAY,
    SKC_EFFECT,
    SKC_FLAGS,
    SKC_GAINRADIUS,
    SKC_GROUP,
    SKC_KEY,
    SKC_NAME,
    SKC_PROMPT_CLILOC,
    SKC_PROMPT_MSG,
    SKC_RANGE,
    SKC_SKILL_GAIN_RANGE,
    SKC_STAT_DEX,
    SKC_STAT_INT,
    SKC_STAT_STR,
    SKC_TITLE,
    SKC_VALUES,
    SKC_QTY
};

lpctstr const CSkillDef::sm_szLoadKeys[SKC_QTY+1] =
{
    "ADV_RATE",
    "BONUS_DEX",
    "BONUS_INT",
    "BONUS_STATS",
    "BONUS_STR",
    "DEFNAME",
    "DELAY",
    "EFFECT",
    "FLAGS",
    "GAINRADIUS",
    "GROUP",
    "KEY",
    "NAME",
    "PROMPT_CLILOC",
    "PROMPT_MSG",
    "RANGE",
    "SKILL_GAIN_RANGE",
    "STAT_DEX",
    "STAT_INT",
    "STAT_STR",
    "TITLE",
    "VALUES",
    nullptr
};

CSkillDef::CSkillDef( SKILL_TYPE skill ) :
    CResourceLink( CResourceID( RES_SKILL, skill ))
{
    m_StatPercent	= 0;
    m_GainRadius	= 0;
    m_Range			= 0;
    m_dwFlags		= 0;
    m_dwGroup		= 0;
    memset(m_Stat, 0, sizeof(m_Stat));
    memset(m_StatBonus, 0, sizeof(m_StatBonus));
    m_AdvRate.Init();
    m_GainRanges.clear();
}

int CSkillDef::GetGainChance(int iCurrentSkill) const
{
    if (m_GainRanges.empty())
    {
        //g_Log.Event(LOGM_DEBUG, "GetGainChance: NO RANGES!\n");
        return 0;
    }
    for (size_t i = 0; i < m_GainRanges.size(); i++)
    {
        const SkillGainRange &range = m_GainRanges[i];
        if (iCurrentSkill >= range.m_iSkillMin && iCurrentSkill <= range.m_iSkillMax)
        {
            return range.m_iChance;
        }
    }
    return 0;
}


bool CSkillDef::r_WriteVal( lpctstr ptcKey, CSString & sVal, CTextConsole * pSrc, bool fNoCallParent, bool fNoCallChildren )
{
    UnreferencedParameter(fNoCallChildren);
    ADDTOCALLSTACK("CSkillDef::r_WriteVal");
    EXC_TRY("WriteVal");
    switch ( FindTableSorted( ptcKey, sm_szLoadKeys, ARRAY_COUNT( sm_szLoadKeys )-1 ))
    {
        case SKC_ADV_RATE:	// ADV_RATE=Chance at 100, Chance at 50, chance at 0
            sVal = m_AdvRate.Write();
            break;
        case SKC_BONUS_DEX: // "BONUS_DEX"
            sVal.FormatVal( m_StatBonus[STAT_DEX] );
            break;
        case SKC_BONUS_INT: // "BONUS_INT"
            sVal.FormatVal( m_StatBonus[STAT_INT] );
            break;
        case SKC_BONUS_STR: // "BONUS_STR"
            sVal.FormatVal( m_StatBonus[STAT_STR] );
            break;
        case SKC_DEFNAME: // "DEFNAME"
            sVal = GetResourceName();
            break;
        case SKC_DELAY:
            sVal = m_vcDelay.Write();
            break;
        case SKC_EFFECT:
            sVal = m_vcEffect.Write();
            break;
        case SKC_FLAGS:
            sVal.FormatHex( m_dwFlags );
            break;
        case SKC_GROUP:
            sVal.FormatHex( m_dwGroup );
            break;
        case SKC_KEY: // "KEY"
            sVal = m_sKey;
            break;
        case SKC_NAME: // "NAME"
            sVal = m_sName.IsEmpty() ? m_sKey : m_sName;
            break;
        case SKC_PROMPT_CLILOC: // "PROMPT_CLILOC"
            sVal = m_sTargetPromptCliloc;
            break;
        case SKC_PROMPT_MSG: // "PROMPT_MSG"
            sVal = m_sTargetPrompt;
            break;
        case SKC_RANGE:
            sVal.FormatVal(m_Range);
            break;
        case SKC_SKILL_GAIN_RANGE:
        {
            CSString sTemp;
            for (size_t i = 0; i < m_GainRanges.size(); i++)
            {
                if (i > 0)
                    sTemp += ";";

                CSString sRange;
                // Ondalık format
                sRange.Format("%.1f,%.1f,%.2f",
                    m_GainRanges[i].m_iSkillMin / 10.0, // 0 -> 0.0
                    m_GainRanges[i].m_iSkillMax / 10.0, // 1000 -> 100.0
                    m_GainRanges[i].m_iChance / 100.0); // 5050 -> 50.50
                sTemp += sRange;
            }
            sVal = sTemp;
        }
        break;
        case SKC_BONUS_STATS: // "BONUS_STATS"
            sVal.FormatVal( m_StatPercent );
            break;
        case SKC_STAT_DEX: // "STAT_DEX"
            sVal.FormatVal( m_Stat[STAT_DEX] );
            break;
        case SKC_STAT_INT: // "STAT_INT"
            sVal.FormatVal( m_Stat[STAT_INT] );
            break;
        case SKC_STAT_STR: // "STAT_STR"
            sVal.FormatVal( m_Stat[STAT_STR] );
            break;
        case SKC_TITLE: // "TITLE"
            sVal = m_sTitle;
            break;
        case SKC_VALUES: // VALUES = 100,50,0 price levels.
            sVal = m_Values.Write();
            break;
        case SKC_GAINRADIUS: // "GAINRADIUS"
            sVal.FormatVal( m_GainRadius );
            break;
        default:
            return ( fNoCallParent ? false : CResourceDef::r_WriteVal( ptcKey, sVal, pSrc ) );
    }
    return true;
    EXC_CATCH;

    EXC_DEBUG_START;
    EXC_ADD_KEYRET(pSrc);
    EXC_DEBUG_END;
    return false;
}

bool CSkillDef::r_LoadVal( CScript &s )
{
    ADDTOCALLSTACK("CSkillDef::r_LoadVal");
    EXC_TRY("LoadVal");
    switch ( FindTableSorted( s.GetKey(), sm_szLoadKeys, ARRAY_COUNT( sm_szLoadKeys )-1 ))
    {
        case SKC_ADV_RATE:	// ADV_RATE=Chance at 100, Chance at 50, chance at 0
            m_AdvRate.Load( s.GetArgStr());
            break;
        case SKC_BONUS_DEX: // "BONUS_DEX"
            m_StatBonus[STAT_DEX] = (uchar)(s.GetArgVal());
            break;
        case SKC_BONUS_INT: // "BONUS_INT"
            m_StatBonus[STAT_INT] = (uchar)(s.GetArgVal());
            break;
        case SKC_BONUS_STR: // "BONUS_STR"
            m_StatBonus[STAT_STR] = (uchar)(s.GetArgVal());
            break;
        case SKC_DEFNAME: // "DEFNAME"
            return SetResourceName( s.GetArgStr());
        case SKC_DELAY:
            m_vcDelay.Load( s.GetArgStr());
            break;
        case SKC_FLAGS:
            m_dwFlags = s.GetArgDWVal();
            break;
        case SKC_GROUP:
            m_dwGroup = s.GetArgDWVal();
            break;
        case SKC_EFFECT:
            m_vcEffect.Load( s.GetArgStr());
            break;
        case SKC_KEY: // "KEY"
            m_sKey = s.GetArgStr();
            return SetResourceName( m_sKey );
        case SKC_NAME: // "KEY"
            m_sName = s.GetArgStr();
            break;
        case SKC_PROMPT_CLILOC: // "PROMPT_CLILOC"
            m_sTargetPromptCliloc = s.GetArgStr();
            break;
        case SKC_PROMPT_MSG: // "PROMPT_MSG"
            m_sTargetPrompt = s.GetArgStr();
            break;
        case SKC_RANGE:
            m_Range = s.GetArgVal();
            break;
        case SKC_SKILL_GAIN_RANGE:
        {
            // m_GainRanges.clear();

            TCHAR szBuffer[256];
            strncpy(szBuffer, s.GetArgStr(), sizeof(szBuffer) - 1);
            szBuffer[sizeof(szBuffer) - 1] = '\0';

            // Token'lara ayır
            TCHAR *pszMin    = strtok(szBuffer, ",");
            TCHAR *pszMax    = strtok(NULL, ",");
            TCHAR *pszChance = strtok(NULL, ",");

            if (!pszMin || !pszMax || !pszChance)
            {
                g_Log.EventError("SKILL_GAIN_RANGE: Invalid format, need 3 values (min,max,chance%%)\n");
                return false;
            }

            // Float olarak parse et
            double dMin    = atof(pszMin);
            double dMax    = atof(pszMax);
            double dChance = atof(pszChance);

            // Validasyon
            if (dMin >= dMax)
            {
                g_Log.EventError("SKILL_GAIN_RANGE: min (%.1f) must be less than max (%.1f)\n", dMin, dMax);
                return false;
            }

            if (dChance < 0.0 || dChance > 100.0)
            {
                g_Log.EventError("SKILL_GAIN_RANGE: chance (%.1f) must be between 0.0-100.0\n", dChance);
                return false;
            }

            SkillGainRange range;
            range.m_iSkillMin = static_cast<int>(dMin * 10);     // 0.0 -> 0, 50.0 -> 500
            range.m_iSkillMax = static_cast<int>(dMax * 10);     // 100.0 -> 1000
            range.m_iChance   = static_cast<int>(dChance * 100); // 50.5 -> 5050 (%50.5)

            m_GainRanges.push_back(range);

            g_Log.Event(LOGM_DEBUG, "SKILL_GAIN_RANGE: [%.1f-%.1f] = %.2f%% (internal: %d-%d=%d)\n", dMin, dMax, dChance, range.m_iSkillMin, range.m_iSkillMax,
                range.m_iChance);
        }
        break;
        case SKC_BONUS_STATS: // "BONUS_STATS"
            m_StatPercent = (uchar)(s.GetArgVal());
            break;
        case SKC_STAT_DEX: // "STAT_DEX"
            m_Stat[STAT_DEX] = (uchar)(s.GetArgVal());
            break;
        case SKC_STAT_INT: // "STAT_INT"
            m_Stat[STAT_INT] = (uchar)(s.GetArgVal());
            break;
        case SKC_STAT_STR: // "STAT_STR"
            m_Stat[STAT_STR] = (uchar)(s.GetArgVal());
            break;
        case SKC_TITLE: // "TITLE"
            m_sTitle = s.GetArgStr();
            break;
        case SKC_VALUES: // VALUES = 100,50,0 price levels.
            m_Values.Load( s.GetArgStr() );
            break;
        case SKC_GAINRADIUS: // "GAINRADIUS"
            m_GainRadius = s.GetArgVal();
            break;

        default:
            return( CResourceDef::r_LoadVal( s ));
    }
    return true;
    EXC_CATCH;

    EXC_DEBUG_START;
    EXC_ADD_SCRIPT;
    EXC_DEBUG_END;
    return false;
}
