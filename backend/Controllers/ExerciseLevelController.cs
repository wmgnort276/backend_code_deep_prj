using backend.Data;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseLevelController : ControllerBase
    {
        private readonly MyDbContext _dbContext;

        public ExerciseLevelController(MyDbContext dbContext) { 
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult All() {
            try
            {
                var exerciseLevels = _dbContext.ExerciseLevels.Select(item => new ExerciseLevelResp
                {
                    Id = item.Id,
                    Name = item.Name,
                    Score = item.Score,
                }).ToList();

                return Ok(exerciseLevels);

            } catch (Exception ex)
            {
                return BadRequest("Fail");
            }
        }

        [HttpPost]
        public IActionResult Add(ExerciseLevelModel exerciseLevel)
        {
            var exerciseFound = _dbContext.ExerciseLevels.SingleOrDefault(item => item.Name == exerciseLevel.Name);
            if(exerciseFound != null)
            {
                return BadRequest("Exercse level exist");
            }

            var newExerciseLevel = new ExerciseLevel
            {
                Name = exerciseLevel.Name,
                Score = exerciseLevel.Score,
            };

            _dbContext.Add(newExerciseLevel);
            _dbContext.SaveChanges();
            return Ok(new ExerciseLevelResp
            {
                Name = newExerciseLevel.Name,
                Score = newExerciseLevel.Score,
            });
        }

    }
}
