using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace ISNAP_4.Models.Entities
{
    public class SimplexTable
    {

        private readonly ISimplexSolver _solver;
        private Vector<double> _solution;

        public Matrix<double> Coefficients { get; }
        public Vector<double> Limitations { get; }
        public Vector<double> Target { get; }

        public SimplexTable(TransportTable table, ISimplexSolver solver)
        {
            _solver = solver;
            
            var simplexTable = FromTransport(table);
            Coefficients = simplexTable.Coefficients;
            Limitations = simplexTable.Limitations;
            Target = simplexTable.Target;
        }

        public SimplexTable(Matrix<double> coefficients, Vector<double> limitations, Vector<double> target)
        {
            Coefficients = coefficients;
            Limitations = limitations;
            Target = target;
        }

        public Vector<double> Solution()
        {
            return _solution ??= _solver.Solve(this);
        }

        private SimplexTable FromTransport(TransportTable table)
        {
            var coefficients = new List<List<double>>();
            var limitations = new List<double>();

            var weights = table.WeightMatrix;
            var columnCounts = weights.ColumnCount * weights.RowCount + 1;

            List<double> CreateCoefficients(List<int> vars)
            {
                var resultRow = new List<double>();
                for (var index = 0; index < columnCounts - 1; index++)
                {
                    resultRow.Add(vars.Contains(index) ? 1 : 0);
                }
                
                return resultRow;
            }

            (List<List<double>> coefficients, List<double> limitations) CreateEquations(Matrix<double> matrix,
                bool isTransposed, List<double> limits)
            {
                var coefficients = new List<List<double>>();
                var limitaitions = new List<double>();

                for (var row = 0; row < matrix.RowCount; row++)
                {
                    var vars = new List<int>();
                    for (var column = 0; column < matrix.ColumnCount; column++)
                    {
                        vars.Add(isTransposed
                            ? row + column * matrix.ColumnCount
                            : column + row * matrix.ColumnCount);
                    }

                    coefficients.Add(CreateCoefficients(vars));
                    limitaitions.Add(limits[row]);
                    
                }

                return (coefficients, limitaitions);
            }

            List<double> CreateTarget(Matrix<double> matrix)
            {
                var result = new List<double>();

                foreach (var element in matrix.Enumerate())
                {
                    result.Add(element);
                }

                return result;
            }
            
            var equations = CreateEquations(weights, false, table.ConsumerNeed.ToList());
            coefficients.AddRange(equations.coefficients);
            limitations.AddRange(equations.limitations);

            var transposedMatrix = weights.Transpose();
            equations = CreateEquations(transposedMatrix, true, table.ProviderHave.ToList());
            coefficients.AddRange(equations.coefficients);
            limitations.AddRange(equations.limitations);

            var target = CreateTarget(weights);
            
            var result = new SimplexTable(Matrix<double>.Build.DenseOfRows(coefficients),
                Vector<double>.Build.DenseOfEnumerable(limitations),
                Vector<double>.Build.DenseOfEnumerable(target));
            
            return result;
        }
    }
}