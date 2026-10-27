using System;
using System.Collections.Generic;
using System.Linq;
public static class NameSolver
{
    private static readonly Dictionary<TroopType, int> troopCode = Enum.GetValues(typeof(TroopType))
        .Cast<TroopType>()
        .ToDictionary(t => t, t => 0);

    private static readonly Dictionary<FacilityType, int> facilityCode = Enum.GetValues(typeof(FacilityType))
        .Cast<FacilityType>()
        .ToDictionary(t => t, t => 0);

    public static string TroopName(TroopType troopType)
    {
        troopCode[troopType]++;
        return troopType.ToString() + troopCode[troopType].ToString();
    }

    public static string FacilityName(FacilityType facility)
    {
        facilityCode[facility]++;
        return facility.ToString() + facilityCode[facility].ToString();
    }

    public static void Reset()
    {
        foreach (var key in troopCode.Keys.ToList())
        {
            troopCode[key] = 0;
        }
        foreach (var key in facilityCode.Keys.ToList())
        {
            facilityCode[key] = 0;
        }
    }
}