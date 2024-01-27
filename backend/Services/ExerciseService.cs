using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection.Metadata;

namespace backend.Services
{
    public class ExerciseService : IExerciseRepository
    {
        private readonly MyDbContext _dbContext;

        public ExerciseService(MyDbContext dbContext) {
            _dbContext = dbContext;
        }
        public ExerciseResp Add(ExerciseModel model, 
            byte[] file,
            byte[] fileJava,
            byte[] testFile, 
            byte[] testFileJava)
        {

          var exerciseLevels = _dbContext.ExerciseLevels.SingleOrDefault(item => item.Id == model.ExerciseLevelId);

          var newExercise = new Exercise
           {
             Name = model.Name,
             Description = model.Description,
             // CreatedAt = DateTime.Now,
             // ModifiedAt = DateTime.Now,
             ExerciseLevelId = model.ExerciseLevelId,
             ExerciseTypeId = model.ExerciseTypeId,
             RunFile = file,
             RunFileJava = fileJava,
             TestFile = testFile,
             TestFileJava = testFileJava,
             HintCode = model.HintCode,
             HintCodeJava = model.HintCodeJava,
             TimeLimit = model.TimeLimit,
             Score = (exerciseLevels?.Score != null) ? exerciseLevels.Score : 0
          };

            _dbContext.Exercises.Add(newExercise);
            _dbContext.SaveChanges();

            var newTestCase1 = new TestCase
            {
                ExerciseId = newExercise.Id,
                Input = model.Input1!,
                Output = model.Output1!,
                Sequence = 1
            };

            var newTestCase2 = new TestCase
            {
                ExerciseId = newExercise.Id,
                Input = model.Input2!,
                Output = model.Output2!,
                Sequence = 2
            };

            var newTestCase3 = new TestCase
            {
                ExerciseId = newExercise.Id,
                Input = model.Input3!,
                Output = model.Output3!,
                Sequence = 3
            };

            _dbContext.TestCases.Add(newTestCase1);
            _dbContext.TestCases.Add(newTestCase2);
            _dbContext.TestCases.Add(newTestCase3);
            _dbContext.SaveChanges();

            return new ExerciseResp
              {
              Id = newExercise.Id,
              Name = newExercise.Name,
              Description = newExercise.Description,
              CreatedAt = newExercise.CreatedAt,
              HintCode = newExercise.HintCode,
              TimeLimit = newExercise.TimeLimit,
            };
            // throw new NotImplementedException();
        }

        public List<ExerciseResp> All(string userId, int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex = 1, int? pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType)
                    .Include(item => item.Submissions)
                    .AsQueryable();

            if (exerciseTypeId != null)
            {
                exercises = exercises.Where(item => item.ExerciseTypeId == exerciseTypeId);
            }

            if (exerciseLevelId != null)
            {
                exercises = exercises.Where(item => item.ExerciseLevelId == exerciseLevelId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                exercises = exercises.Where(item => item.Name.ToLower().Contains(keyword.ToLower()));
            }

            //var result = PaginatedList<Exercise>.Create(exercises, pageIndex, pageSize);
            return exercises.Select(item => new ExerciseResp
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ExerciseLevelId = item.ExerciseLevelId,
                ExerciseTypeId=item.ExerciseTypeId,
                ExerciseLevelName = item.ExerciseLevel.Name,
                ExerciseTypeName = item.ExerciseType.Name,
                IsResolved = item.Submissions.Any(s => s.ExerciseId == item.Id && s.StudentId == userId && s.Status),
            }).ToList();

            //   throw new NotImplementedException();
        }

        public ExerciseResp? GetById(Guid id, string userId)
        {
            var exercise = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType)
                    .SingleOrDefault(item => item.Id == id);

            

            if (exercise != null)
            {
                var ratings = _dbContext.Rating.Where(rating => rating.ExerciseId == id)
                    .ToList();

                double averageRating = 0;
                int ratingCount = 0;
                if(ratings.Any())
                {
                    averageRating = ratings.Average(item => item.RatingValue);
                    ratingCount = ratings.Count();
                }

                var testCases = _dbContext.TestCases
                    .Where(item => item.ExerciseId == exercise.Id)
                    .ToList();
                string input1 = String.Empty;
                string input2 = String.Empty;
                string input3 = String.Empty;
                string output1 = String.Empty;
                string output2 = String.Empty;
                string output3 = String.Empty;

                if (testCases.Any())
                {
                    foreach (var item in testCases)
                    {
                        if(item.Sequence == 1)
                        {
                            input1 = item.Input;
                            output1 = item.Output;
                        }
                        else if (item.Sequence == 2)
                        {
                            input2 = item.Input;
                            output2 = item.Output;
                        }
                        else if (item.Sequence == 3)
                        {
                            input3 = item.Input;
                            output3 = item.Output;
                        }
                    }
                }


                return new ExerciseResp
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    ExerciseLevelId = exercise.ExerciseLevelId,
                    ExerciseTypeId = exercise.ExerciseTypeId,
                    // RunFile = exercise.RunFile,
                    ExerciseLevelName = exercise.ExerciseLevel.Name,
                    ExerciseTypeName = exercise.ExerciseType.Name,
                    HintCode = exercise.HintCode,
                    HintCodeJava = exercise.HintCodeJava,
                    TimeLimit = exercise.TimeLimit,
                    RatingCount = ratingCount,
                    Rating = averageRating,
                    Input1 = input1,
                    Input2 = input2,
                    Input3 = input3,
                    Output1 = output1,
                    Output2 = output2,
                    Output3 = output3,
                };
            }

            return null;
            // throw new NotImplementedException();
        }

        public byte[]? DownLoadrunFille(Guid id)
        {
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);

            if (exercise != null)
            {
                return exercise?.RunFile;
            }

            return null;
        }

        public string Submit(Guid id, string userId, SourceCode sourceCode)
        {
            int DEFAULT_TIME_LIMIT = 20000;
            var code = sourceCode.Code;
           
            string fileName = Guid.NewGuid().ToString().Replace("-", ""); ;

            string filePath = string.Empty;
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            string compileResult = string.Empty;
            string executionResult = string.Empty;
            string errorExecutionResult = string.Empty;

            int limitTime = 0;
            int runTime = 0;
            int memory = 0;
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);
            if(exercise != null)
            {
                limitTime = exercise.TimeLimit;
            }
            var isFirstSubmit = CheckUserFirstSubmit(userId, exercise.Id);

            try
            {

                // Check source code

                if (!CheckSourceCode(code))
                {
                    throw new InvalidOperationException("Source code contain sensitive function name! Checkout the term of use page for details!");
                }

                //////
                
                string program = string.Empty;
                string arguments = string.Empty;

                if (sourceCode.Lang == "C++")
                {
                  filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".cpp");
                  program = "g++";
                  arguments = $"{filePath} -o {compiledFilePath}";
                  compileResult = CompileCppCode(filePath, exercise.RunFile, code, program, arguments, "");
                } else
                {
                  filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".java");
                  program = "javac";
                  arguments = $"{filePath}";
                  compileResult = CompileJavaCode(filePath, exercise.RunFileJava, code, program, arguments, fileName,"");
                  compiledFilePath = Directory.GetCurrentDirectory();
                }

                if (!string.IsNullOrEmpty(compileResult))
                {
                    string cleanedErrorOutput = compileResult.Replace($"{filePath}:", "");
                    throw new Exception(cleanedErrorOutput);
                }


                ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compiledFilePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                ProcessStartInfo executionJavaProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-cp {compiledFilePath} Main{fileName}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                using (Process executionProcess = new Process())
                {
                    // TODO: use Stopwatch to calculate time running instead
                    var startTime = DateTime.Now;
                    Stopwatch stopwatch = new Stopwatch();
                    executionProcess.StartInfo = (sourceCode.Lang == "C++") ? executionProcessStartInfo : executionJavaProcessStartInfo;

                    // Start watiching process
                    stopwatch.Start();
                    executionProcess.Start();
                    memory = (int)executionProcess.PrivateMemorySize64;

                    var task = Task.Run(() =>
                    {
                        while (!executionProcess.HasExited)
                        {
                            memory = (int)executionProcess.PrivateMemorySize64;
                            executionProcess.Refresh();
                        }
                    });


                    // TODO: change the time waiting
                    bool isProcessStop = executionProcess.WaitForExit((limitTime > 0) ? limitTime : DEFAULT_TIME_LIMIT);
                    stopwatch.Stop();
                    runTime = (int)stopwatch.ElapsedMilliseconds;

                    if (!isProcessStop)
                    {
                        Console.WriteLine("Not finish yet!");
                        throw new TimeoutException("Execution time out!");
                    }

                    executionResult = executionProcess.StandardOutput.ReadToEnd();
                    errorExecutionResult = executionProcess.StandardError.ReadToEnd();
                    // runTime = (int)(executionProcess.ExitTime - startTime).TotalMilliseconds;
                }

                return executionResult;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ex.Message;
            }
            finally
            {
                Console.WriteLine("Memory: " + memory);
                // when the answer is right, update data to submission table
                var submission = new SubmissionModel
                {
                    Status = (executionResult == "1"),
                    StudentId = userId,
                    ExerciseId = exercise.Id,
                    Runtime = runTime,
                    Memory = memory,
                };

                AddSubmission(submission, code);
                
                // Update user score only it is the first time user submit 
                if(executionResult == "1" && isFirstSubmit)
                {
                    AddUserScore(userId, exercise.Score);
                }

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                if (System.IO.File.Exists(compiledFilePath + ".exe"))
                {
                    foreach (var process in Process.GetProcessesByName(fileName))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }

                    System.IO.File.Delete(compiledFilePath + ".exe");

                }

                if (System.IO.File.Exists(Path.Combine(compiledFilePath, $"Main{fileName}.class")))
                {
                    try
                    {
                       System.IO.File.Delete(Path.Combine(compiledFilePath, $"Main{fileName}.class"));
                    } catch(Exception ex)
                    {
                        Console.WriteLine("error delete file!");
                    }
                }
            }

            throw new NotImplementedException();
        }

        public ExerciseResp Edit(ExerciseModel model, byte[] file, byte[] fileJava, byte[] testFile, byte[] testFileJava)
        {
            // TODO: only level change then we find exercise level
            var exerciseLevels = _dbContext.ExerciseLevels.SingleOrDefault(item => item.Id == model.ExerciseLevelId);
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == model.Id);

            if(exercise == null)
            {
                throw new Exception("Not found");
            }

            exercise.Name = model.Name;
            exercise.Description = model.Description;
            exercise.ExerciseLevelId = model.ExerciseLevelId;
            exercise.ExerciseTypeId = model.ExerciseTypeId;
            exercise.HintCode = model.HintCode;
            exercise.HintCodeJava = model.HintCodeJava;
            exercise.TimeLimit = model.TimeLimit;
            exercise.Score = (exerciseLevels?.Score != null) ? exerciseLevels.Score : 0;
            

            if (file != null && file.Length > 0)
            {
                exercise.RunFile = file;
            }
            if (fileJava != null && fileJava.Length > 0)
            {
                exercise.RunFileJava = fileJava;
            }
            if (testFile != null && testFile.Length > 0)
            {
                exercise.TestFile = testFile;
            }
            if (testFileJava != null && testFileJava.Length > 0)
            {
                exercise.TestFileJava = testFileJava;
            }
            _dbContext.SaveChanges();

            var listTestCase = _dbContext.TestCases
                .Where(item => item.ExerciseId ==  exercise.Id)
                .ToList();

            if(listTestCase.Any())
            {
                foreach (var item in listTestCase)
                {
                    if (item.Sequence == 1)
                    {
                        item.Input = model.Input1;
                        item.Output = model.Output1;
                    }
                    else if (item.Sequence == 2)
                    {
                        item.Input = model.Input2;
                        item.Output = model.Output2;
                    }
                    else if (item.Sequence == 3)
                    {
                        item.Input = model.Input3;
                        item.Output = model.Output3;
                    }
                }
                _dbContext.SaveChanges();

            } else
            {
                // Insert new data to table
                var newTestCase1 = new TestCase
                {
                    ExerciseId = exercise.Id,
                    Input = model.Input1!,
                    Output = model.Output1!,
                    Sequence = 1
                };

                var newTestCase2 = new TestCase
                {
                    ExerciseId = exercise.Id,
                    Input = model.Input2!,
                    Output = model.Output2!,
                    Sequence = 2
                };

                var newTestCase3 = new TestCase
                {
                    ExerciseId = exercise.Id,
                    Input = model.Input3!,
                    Output = model.Output3!,
                    Sequence = 3
                };

                _dbContext.TestCases.Add(newTestCase1);
                _dbContext.TestCases.Add(newTestCase2);
                _dbContext.TestCases.Add(newTestCase3);
                _dbContext.SaveChanges();
            }

            return new ExerciseResp
            {
                Name = exercise.Name,
                Description = exercise.Description,
                ExerciseLevelId = exercise.ExerciseLevelId,
                ExerciseTypeId = exercise.ExerciseTypeId,
                HintCode = exercise.HintCode,
                TimeLimit = exercise.TimeLimit,
            };
        }

        public SubmissionResp AddSubmission(SubmissionModel model, string scourceCode)
        {
            var newSubmission = new Submission
            {
                Status = model.Status,
                StudentId = model.StudentId,
                ExerciseId = model.ExerciseId,
                Runtime = model.Runtime,
                Memory = model.Memory,
                SourceCode = scourceCode,
            };

            _dbContext.Add(newSubmission);
            _dbContext.SaveChanges();

            return new SubmissionResp
            {
                Status = newSubmission.Status,
                StudentId = newSubmission.StudentId,
                ExerciseId = newSubmission.ExerciseId
            };
        }

        public void AddUserScore(string userId, int score)
        {

            // To do check if first submission
             var currentUser = _dbContext.Users.SingleOrDefault(item => item.Id == userId);
             currentUser.Score += score;
             _dbContext.SaveChanges();
           
        }

        public bool CheckUserFirstSubmit(string userId, Guid exerciseId)
        {
            var submissionFount = _dbContext.Submissions
                .Where(item => item.StudentId == userId && item.ExerciseId == exerciseId && item.Status).ToList();
            if(submissionFount != null && submissionFount.Count() > 0)
            {
                return false;
            }

            return true;
        }

        public string CompileCppCode(string filePath, 
            byte[] runfile, 
            string souceCode, 
            string program, 
            string arguments,
            string testCaseInput,
            bool ISCheckTestCase = false
            )
        {
            File.WriteAllBytes(filePath, runfile);

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length >= 8)
            {
                lines[7] = souceCode;
                File.WriteAllLines(filePath, lines);
            }

            if(ISCheckTestCase)
            {
                PrepareTestCase(filePath, testCaseInput);
            }

            string compileOutput = string.Empty;

            ProcessStartInfo compileProcessStartInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            using (Process compileProcess = new Process())
            {
                compileProcess.StartInfo = compileProcessStartInfo;
                compileProcess.Start();
                compileProcess.WaitForExit();
                compileOutput = compileProcess.StandardError.ReadToEnd();
            }

            return compileOutput;
        }

        public string CompileJavaCode(string filePath, 
            byte[] runfile, 
            string souceCode, 
            string program, 
            string arguments, 
            string fileName, 
            string testCaseInput,
            bool ISCheckTestCase = false
            )
        {
            File.WriteAllBytes(filePath, runfile);

            string[] lines = File.ReadAllLines(filePath);


            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("class Main"))
                {
                    lines[i+1] = souceCode;
                }
                lines[i] = lines[i].Replace("class Main", $"class Main{fileName}");
            }

            File.WriteAllLines(filePath, lines);

            if (ISCheckTestCase)
            {
                PrepareTestCaseJava(filePath, testCaseInput);
            }

            string compileOutput = string.Empty;

            ProcessStartInfo compileProcessStartInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            using (Process compileProcess = new Process())
            {
                compileProcess.StartInfo = compileProcessStartInfo;
                compileProcess.Start();
                compileProcess.WaitForExit();
                compileOutput = compileProcess.StandardError.ReadToEnd();
            }

            return compileOutput;
        }


        public List<TestCaseResp> CheckTestCase(Guid exerciseId, SourceCode sourceCode)
        {
            int DEFAULT_TIME_LIMIT = 20000;
            var code = sourceCode.Code;


            string compileResult = string.Empty;
            string executionResult = string.Empty;
            string errorExecutionResult = string.Empty;

            int limitTime = 0;

            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == exerciseId);
            if (exercise != null)
            {
                limitTime = exercise.TimeLimit;
            }

            var listTestCase = _dbContext.TestCases
               .Where(item => item.ExerciseId == exercise.Id)
               .ToList();

            List<TestCaseResp> listTestCaseRes = new List<TestCaseResp>();

            string fileName = Guid.NewGuid().ToString().Replace("-", ""); ;

            string filePath = string.Empty;
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            for (int i = 0; i < 3; i++)
            {

                try
                {
                    if (!CheckSourceCode(code))
                    {
                        throw new InvalidOperationException("Source code contain sensitive function name! Checkout the term of use page for details!");
                    }
                    string program = string.Empty;
                    string arguments = string.Empty;

                    if (sourceCode.Lang == "C++")
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".cpp");
                        program = "g++";
                        arguments = $"{filePath} -o {compiledFilePath}";
                        compileResult = CompileCppCode(filePath, exercise.TestFile, code, program, arguments, listTestCase[i].Input, true);
                    }
                    else
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".java");
                        program = "javac";
                        arguments = $"{filePath}";
                        compileResult = CompileJavaCode(filePath, exercise.TestFileJava, code, program, arguments, fileName, listTestCase[i].Input, true);
                        compiledFilePath = Directory.GetCurrentDirectory();
                    }

                    if (!string.IsNullOrEmpty(compileResult))
                    {
                        string cleanedErrorOutput = compileResult.Replace($"{filePath}:", "");
                        throw new Exception(cleanedErrorOutput);
                    }


                    ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                    {
                        FileName = compiledFilePath,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                    };

                    ProcessStartInfo executionJavaProcessStartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = $"-cp {compiledFilePath} Main{fileName}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                    };


                    using (Process executionProcess = new Process())
                    {
                        var startTime = DateTime.Now;
                        executionProcess.StartInfo = (sourceCode.Lang == "C++") ? executionProcessStartInfo : executionJavaProcessStartInfo;

                        // Start watiching process
                        executionProcess.Start();


                        // TODO: change the time waiting
                        bool isProcessStop = executionProcess.WaitForExit((limitTime > 0) ? limitTime : DEFAULT_TIME_LIMIT);

                        if (!isProcessStop)
                        {
                            Console.WriteLine("Not finish yet!");
                            throw new TimeoutException("Execution timed out");
                        }

                        executionResult = executionProcess.StandardOutput.ReadToEnd();
                        errorExecutionResult = executionProcess.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(errorExecutionResult))
                        {
                            string cleanedErrorOutput = errorExecutionResult.Replace($"{filePath}:", "");
                            throw new Exception(cleanedErrorOutput);
                        }

                    }

                    var testCaseRes = new TestCaseResp
                    {
                        Input = listTestCase[i].Input,
                        Output = executionResult,
                        Expected = listTestCase[i].Output,
                        Status = executionResult.Equals(listTestCase[i].Output)
                    };

                    Console.Write("====== Output: " + executionResult);
                    Console.Write("====== Expected : " + listTestCase[i].Output);

                    listTestCaseRes.Add(testCaseRes);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    var testCaseRes = new TestCaseResp
                    {
                        Input = listTestCase[i].Input,
                        Output = ex.Message,
                        Expected = listTestCase[i].Output,
                        Status = false
                    };

                    listTestCaseRes.Add(testCaseRes);
                }
                finally
                {

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    if (System.IO.File.Exists(compiledFilePath + ".exe"))
                    {
                        foreach (var process in Process.GetProcessesByName(fileName))
                        {
                            process.Kill();
                            process.WaitForExit();
                        }

                        System.IO.File.Delete(compiledFilePath + ".exe");

                    }

                    if (System.IO.File.Exists(Path.Combine(compiledFilePath, $"Main{fileName}.class")))
                    {
                        try
                        {
                            System.IO.File.Delete(Path.Combine(compiledFilePath, $"Main{fileName}.class"));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("error delete file!");
                        }
                    }
                }

            }

            return listTestCaseRes;

        }

        public void PrepareTestCase(string filename, string input)
        {
            string targetString = "string TESTCASE;";


            string newValue = $"string TESTCASE = \"{input}\";";

            try
            {
                string[] lines = File.ReadAllLines(filename);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(targetString))
                    {
                        lines[i] = newValue;
                        break;
                    }
                }

                File.WriteAllLines(filename, lines);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Cannot open file test case: " + filename);
                return;
            }

        }

        public void PrepareTestCaseJava(string filename, string input)
        {
            string targetString = "static String TESTCASE;";


            string newValue = $"static String TESTCASE = \"{input}\";";

            try
            {
                string[] lines = File.ReadAllLines(filename);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(targetString))
                    {
                        lines[i] = newValue;
                        break;
                    }
                }

                File.WriteAllLines(filename, lines);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Cannot open file test case: " + filename);
                return;
            }

        }



        public bool CheckSourceCode(string sourceCode)
        {
            string[] forbiddenStrings = new ForbiddenStringsBuilder()
                .AddGroup(FileActionForbidden())
                .AddGroup(NetowrkForbidden())
                .AddGroup(SystemForbidden())
                .Build();

            foreach (string forbiddenString in forbiddenStrings)
            {
                if (sourceCode.Contains(forbiddenString))
                {
                    return false;
                }
            }

            return true;
        }

        public class ForbiddenStringsBuilder
        {
            private List<string> forbiddenStrings = new List<string>();

            public ForbiddenStringsBuilder AddGroup(string[] group)
            {
                forbiddenStrings.AddRange(group);
                return this;
            }

            public ForbiddenStringsBuilder AddString(string str)
            {
                forbiddenStrings.Add(str);
                return this;
            }

            public string[] Build()
            {
                return forbiddenStrings.ToArray();
            }
        }

        public string[] FileActionForbidden()
        {
            return new string[] { "remove(", "delete(" };
        }

        public string[] NetowrkForbidden()
        {
            return new string[] { "socket(", "connect(" };
        }

        public string[] SystemForbidden()
        {
            return new string[] { "exec(", "process(", "pause(", "sleep(", "fork(" };
        }

        public List<ExerciseResponseAdmin> AllForAdmin(string userId, int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex = 1, int? pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType)
                    .Include(item => item.Submissions)
                    .Include(item => item.Ratings)
                    .AsQueryable();

            if (exerciseTypeId != null)
            {
                exercises = exercises.Where(item => item.ExerciseTypeId == exerciseTypeId);
            }

            if (exerciseLevelId != null)
            {
                exercises = exercises.Where(item => item.ExerciseLevelId == exerciseLevelId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                exercises = exercises.Where(item => item.Name.ToLower().Contains(keyword.ToLower()));
            }

            //var result = PaginatedList<Exercise>.Create(exercises, pageIndex, pageSize);
            return exercises.Select(item => new ExerciseResponseAdmin
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ExerciseLevelId = item.ExerciseLevelId,
                ExerciseTypeId = item.ExerciseTypeId,
                ExerciseLevelName = item.ExerciseLevel.Name,
                ExerciseTypeName = item.ExerciseType.Name,
                SubmittedNumber = item.Submissions.Where(s => s.ExerciseId == item.Id && s.Status).ToList().Count,
            }).ToList();
        }
    }
}
