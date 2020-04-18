using ISNAP_4.Models.Entities;
using ISNAP_4.Models.PythonSolver;
using NUnit.Framework;

namespace TestProject
{
    public class TransportTableTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void InitTest()
        {
            
            //Закрытая задча            
            var matrix1 = new[]
            {
                new[] {3.0, 1.0},
                new[] {2.0, 6.0}
            };

            var have1 = new[] {20.0, 20.0};
            var need1 = new[] {30.0, 10.0};

            var table1 = new TransportTable(matrix1, have1, need1);

            double[,] actMatrix1 = {{3.0, 1.0}, {2.0, 6.0}};

            Assert.AreEqual(table1.WeightMatrix.ToArray(), actMatrix1);
            Assert.AreEqual(table1.ConsumerNeed.ToArray(), need1);
            Assert.AreEqual(table1.ProviderHave.ToArray(), have1);
            
            //Открытая задча (нужно меньше, чем есть)   
            var matrix2 = new[]
            {
                new[] {3.0, 1.0},
                new[] {2.0, 6.0}
            };

            var have2 = new[] {20.0, 20.0};
            var need2 = new[] {20.0, 10.0};
            
            var table2 = new TransportTable(matrix2, have2, need2);
            var actHave2 = new[] {20.0, 20.0};
            var actNeed2 = new[] {20.0, 10.0, 10};
            
            double[,] actMatrix2 = {{3.0, 1.0}, {2.0, 6.0}, {0.0, 0.0}};

            Assert.AreEqual(table2.WeightMatrix.ToArray(), actMatrix2);
            Assert.AreEqual(table2.ConsumerNeed.ToArray(), actNeed2);
            Assert.AreEqual(table2.ProviderHave.ToArray(), actHave2);
            
            //Открытая задча (нужно больше, чем есть)  
            var matrix3 = new[]
            {
                new[] {3.0, 1.0},
                new[] {2.0, 6.0}
            };

            var have3 = new[] {20.0, 10.0};
            var need3 = new[] {20.0, 20.0};
            
            var table3 = new TransportTable(matrix3, have3, need3);
            var actHave3 = new[] {20.0, 10.0, 10.0};
            var actNeed3 = new[] {20.0, 20.0};
            
            double[,] actMatrix3 = {{3.0, 1.0, 7.0}, {2.0, 6.0, 7.0}};

            Assert.AreEqual(table3.WeightMatrix.ToArray(), actMatrix3);
            Assert.AreEqual(table3.ConsumerNeed.ToArray(), actNeed3);
            Assert.AreEqual(table3.ProviderHave.ToArray(), actHave3);
        }

        [Test]
        public void SolutionTest1()
        {
            var matrix = new[]
            {
                new [] {2.0, 3.0, 1.0},
                new [] {4.0, 2.0, 5.0}
            };

            double[] need = {30.0, 200.0};
            double[] have = {20.0, 50.0, 100.0};

            double[,] actual = { {0,0, 30, 0}, {20, 50, 70, 60} };
            
            var table = new TransportTable(matrix, have, need);

            var solver = new PythonSimplexSolver();
            var solution = table.SolveTable(solver);
            
            Assert.AreEqual(table.FunctionValue(solver), 920);
            Assert.AreEqual(solution.ToArray(), actual);
        }
        
        [Test]
        public void SolutionTest2()
        {
            var matrix = new[]
            {
                new [] {20.0, 29.0, 6.0},
                new [] {23.0, 15.0, 11.0},
                new [] {20.0, 16.0, 10.0},
                new [] {15.0, 19.0, 9.0},
                new [] {24.0, 29.0, 8.0},
            };

            double[] need = {150.0, 140.0, 110.0, 230.0, 220.0};
            double[] have = {320.0, 280.0, 250.0};

            double[,] actual = { { 120.0, 0.0, 30.0}, {0.0, 140.0, 0.0}, {0.0, 110.0, 0.0}, {200.0, 30.0, 0.0}, {0.0, 0.0, 220.0} };
            
            var table = new TransportTable(matrix, have, need);

            var pythonSolver = new PythonSimplexSolver();
            
            var solution = table.SolveTable(pythonSolver);
            
            Assert.AreEqual(table.FunctionValue(pythonSolver), 11770);
            Assert.AreEqual(solution.ToArray(), actual);
        } 
        
        [Test]
        public void SolutionTest3()
        {
            var matrix = new[]
            {
                new [] {4.0, 4.0, 3.0},
                new [] {3.0, 2.0, 4.0},
            };

            double[] need = { 26.0, 33.0};
            double[] have = {22.0, 21.0, 24.0};

            double[,] actual = { { 2.0, 0.0, 24.0}, {12.0, 21.0, 0.0}, {8.0, 0.0, 0.0} };
            
            var table = new TransportTable(matrix, have, need);

            var pythonSolver = new PythonSimplexSolver();
            
            var solution = table.SolveTable(pythonSolver);
            
            Assert.AreEqual(table.FunctionValue(pythonSolver), 158);
            Assert.AreEqual(solution.ToArray(), actual);
        }
    }
}