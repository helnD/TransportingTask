using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace ISNAP_4.Models.Entities
{
    public class TransportTable
    {

        private Matrix<double> _minSolution;
        private double? _functionValue;

        public TransportTable(double[][] weightMatrix, double[] providerHave, double[] consumerNeed)
        {
            var forClosed = MakeClose(weightMatrix, providerHave, consumerNeed);
            
            ProviderHave = Vector<double>.Build.DenseOfArray(forClosed.Provider);
            ConsumerNeed = Vector<double>.Build.DenseOfArray(forClosed.Consumer);
            WeightMatrix = Matrix<double>.Build.DenseOfRowArrays(forClosed.Weight);
        }
        
        public Matrix<double> WeightMatrix { get; }
        public Vector<double> ProviderHave { get; }
        public Vector<double> ConsumerNeed { get; }

        public Matrix<double> SolveTable(ISimplexSolver solver)
        {

            if (_minSolution != null) return _minSolution;
            
            var simplexTable = new SimplexTable(this, solver);
            var solution = simplexTable.Solution();

            _minSolution = SolutionToMatrix(solution);
            _functionValue = CalculateFunction(_minSolution);

            return _minSolution;
        }

        public double FunctionValue(ISimplexSolver solver)
        {
            if (_functionValue != null) return _functionValue.Value;
            
            var simplexTable = new SimplexTable(this, solver);
            
            _minSolution = SolutionToMatrix(simplexTable.Solution());
            _functionValue = CalculateFunction(_minSolution);

            return _functionValue.Value;
        }
        
        //приводит задачу к закрытому виду
        private (double[][] Weight, double[] Provider, double[] Consumer) MakeClose(double[][] weightMatrix, 
            double[] providerHave, double[] consumerNeed)
        {
            var resultMatrix = weightMatrix.Select(it => it.ToList()).ToList();
            var resultHave = providerHave.ToList();
            var resultNeed = consumerNeed.ToList();

            var diff = providerHave.Sum() - consumerNeed.Sum();

            //Имеем больше, чем нужно
            if (diff > 0)
            {
                resultNeed.Add(diff);
                var newRow = new List<double>();
                newRow.AddRange(new double[resultHave.Count]);

                resultMatrix.Add(newRow);
            }

            //Имеем меньше, чем нужно
            if (diff < 0)
            {
                var maxElement = resultMatrix.Select(it => it.Max()).Max();

                resultHave.Add(-diff);
                resultMatrix.ForEach(it => it.Add(maxElement + 1));
            }

            return (resultMatrix.Select(it => it.ToArray()).ToArray(), resultHave.ToArray(), resultNeed.ToArray());
        }

        private Matrix<double> SolutionToMatrix(Vector<double> solution)
        {
            var result = new List<List<double>>();

            for (var row = 0; row < WeightMatrix.RowCount; row++)
            {
                var newRow = new List<double>();
                for (var col = 0; col < WeightMatrix.ColumnCount; col++)
                {
                    newRow.Add(solution[row * WeightMatrix.ColumnCount + col]);
                }
                result.Add(newRow);
            }

            return Matrix<double>.Build.DenseOfRows(result);
        }

        private double CalculateFunction(Matrix<double> solution)
        {
            var sum = 0.0;

            for (var row = 0; row < solution.RowCount; row++)
            {
                for (var col = 0; col < solution.ColumnCount; col++)
                {
                    sum += solution[row, col] * WeightMatrix[row, col];
                }
            }

            return sum;
        }
        
    }
    
}