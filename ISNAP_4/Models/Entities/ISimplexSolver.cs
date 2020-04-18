using System.Collections.Immutable;
using MathNet.Numerics.LinearAlgebra;

namespace ISNAP_4.Models.Entities
{
    public interface ISimplexSolver
    {
        Vector<double> Solve(SimplexTable table);
    }
}