using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

public sealed class InMemoryEvaluationRepository : IEvaluationRepository
{
    private readonly Dictionary<EvaluationId, Evaluation> _items = [];

    public Evaluation? FindById(EvaluationId id) => _items.GetValueOrDefault(id);

    public Evaluation? Find(StudentId studentId, CriterionId criterionId) =>
        _items.Values.FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);

    public IReadOnlyList<Evaluation> GetAll() => _items.Values.ToList();

    public IReadOnlyList<Evaluation> GetByStudent(StudentId studentId) =>
        _items.Values.Where(e => e.StudentId == studentId).ToList();

    public void Save(Evaluation evaluation) => _items[evaluation.Id] = evaluation;

    public void Remove(EvaluationId id) => _items.Remove(id);

    public int RemoveByStudent(StudentId studentId) => RemoveWhere(e => e.StudentId == studentId);

    public int RemoveByCriterion(CriterionId criterionId) => RemoveWhere(e => e.CriterionId == criterionId);

    private int RemoveWhere(Func<Evaluation, bool> predicate)
    {
        List<EvaluationId> ids = _items.Values.Where(predicate).Select(e => e.Id).ToList();
        ids.ForEach(id => _items.Remove(id));
        return ids.Count;
    }
}
