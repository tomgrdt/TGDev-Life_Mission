using System;
using System.Collections.Generic;
using System.Text;

using LifeMissionModels.Models;
using SharedProjectDatabase.Database;
using SharedProjectDatabase.Database.LifeMission;

namespace SharedProjectDatabase.Functions.LifeMission;

public static partial class DatabaseFunctions
{
    #region Add

    public static int AddQuest(QuestModel quest)
    {
        int questId = DatabaseManager.Add(Convert(quest));

        return questId;
    }

    #endregion

    #region Get

    public static QuestModel? Getquest(int id)
    {
        Quest? quest = DatabaseManager.Get<Quest>().FirstOrDefault(x => x.Id == id);

        return quest == null ? null : Convert(quest);
    }

    public static List<QuestModel> GetquestModels()
    {
        List<QuestModel> questModels = new List<QuestModel>();
        List<Quest> quests = DatabaseManager.Get<Quest>().ToList();

        quests.ForEach(quest =>
        {
            questModels.Add(Convert(quest));
        });

        return questModels;
    }

    public static List<Quest> Getquests()
    {
        return DatabaseManager.Get<Quest>().ToList();
    }

    public static QuestModel GetQuestModelFromQuest(Quest quest)
    {
        return Convert(quest);
    }
    #endregion

    #region Update

    public static void Updatequest(QuestModel quest)
    {
        DatabaseManager.Update(Convert(quest));
    }

    #endregion

    #region Converts

    private static QuestModel Convert(Quest quest)
    {
        return new QuestModel
        {
            Id = quest.Id,
        };
    }

    private static Quest Convert(QuestModel questModel)
    {
        Quest quest = new Quest
        {
            Id = questModel.Id,
        };

        return quest;
    }

    #endregion
}
