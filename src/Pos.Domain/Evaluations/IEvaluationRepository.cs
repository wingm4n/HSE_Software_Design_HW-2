using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

public interface IEvaluationRepository
{
    Evaluation? FindById(EvaluationId id);

    Evaluation? Find(StudentId studentId, CriterionId criterionId);

    IReadOnlyList<Evaluation> GetAll();

    IReadOnlyList<Evaluation> GetByStudent(StudentId studentId);

    void Save(Evaluation evaluation);

    void Remove(EvaluationId id);

    /// <returns>Number of removed evaluations.</returns>
    int RemoveByStudent(StudentId studentId);

    /// <returns>Number of removed evaluations.</returns>
    int RemoveByCriterion(CriterionId criterionId);
}
