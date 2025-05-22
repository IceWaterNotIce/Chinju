using System.Collections.Generic;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    private List<Mission> missions = new List<Mission>();

    public IReadOnlyList<Mission> Missions => missions;

    public void AddMission(Mission mission)
    {
        if (!missions.Exists(m => m.Id == mission.Id))
            missions.Add(mission);
    }

    public void CompleteMission(string missionId)
    {
        var mission = missions.Find(m => m.Id == missionId);
        if (mission != null)
            mission.Complete();
    }

    public Mission GetMission(string missionId)
    {
        return missions.Find(m => m.Id == missionId);
    }
}
