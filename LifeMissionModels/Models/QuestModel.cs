using LifeMissionModels.Enums;

namespace LifeMissionModels.Models;

public class QuestModel
{
    public int Id { get; set; }
    public string QuestId { get; set; } = "";
    public string Title { get; set; } = "";
    public int Xp { get; set; }
    public string Desc { get; set; } = "";
    public List<SubTaskModel> Subtasks { get; set; } = new();
    public QuestStatus Status { get; set; } = QuestStatus.Locked;
}
