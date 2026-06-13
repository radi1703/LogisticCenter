using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsSystem.Models;

namespace LogisticsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentsController : ControllerBase
    {
        private readonly LogisticsCenterContext _context;

        public ShipmentsController(LogisticsCenterContext context)
        {
            _context = context;
        }

        //Вземане на всички пратки
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipments()
        {
            return await _context.Shipments.ToListAsync();
        }

        //Търсене по номер
        [HttpGet("search/{trackingNumber}")]
        public async Task<ActionResult<Shipment>> GetByTrackingNumber(string trackingNumber)
        {
            var shipment = await _context.Shipments
                .Include(s => s.AssignedCourier) //Зареждаме и данните за куриера
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

            if (shipment == null)
            {
                return NotFound(new { message = "Пратката не е намерена" });
            }

            return shipment;
        }

        //създаване на пратка - оператор
        [HttpPost("create")]
        public async Task<ActionResult<Shipment>> CreateShipment(ShipmentCreateDto newData)
        {
            if (newData.Weight == null || newData.Weight <= 0){
                return BadRequest(new { message = "Моля, въведете валидно тегло на пратката, по-голямо от 0 кг!" });
            }

            if (newData.AssignedCourierId.HasValue)
            {
                // Броим колко пратки има този куриер със статус "В движение"
                var activeCount = await _context.Shipments
                    .CountAsync(s => s.AssignedCourierId == newData.AssignedCourierId.Value && s.Status == "In Transit");

                if (activeCount >= 5) // Лимитът е 5 пратки едновременно
                {
                    return BadRequest(new { message = "ВНИМАНИЕ: Този куриер е достигнал лимита си от 5 пратки! Моля, изберете друг куриер или оставете пратката в склад." });
                }
            }

            // Генериране на случаен номер
            var rnd = new Random();
            string newTracking = "BG" + rnd.Next(100000, 999999).ToString();

            var shipment = new Shipment
            {
                TrackingNumber = newTracking,
                SenderName = newData.SenderName,
                SenderPhone = newData.SenderPhone,
                ReceiverName = newData.ReceiverName,
                ReceiverPhone = newData.ReceiverPhone,
                ReceiverAddress = newData.ReceiverAddress,
                Weight = newData.Weight,
                
                CreatedDate = DateTime.Now,
                CreatedBy = newData.CreatedBy, // Записваме кой я е създал
                
                // Ако има избран куриер пратката е "In Transit", а ако няма е в "In Warehouse".
                Status = newData.AssignedCourierId.HasValue ? "In Transit" : "In Warehouse",
        
                AssignedCourierId = newData.AssignedCourierId, // Даваме я на избрания куриер
                //ако не в куриер, отива в склада
                CurrentWarehouseId = newData.AssignedCourierId.HasValue ? null : 1
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync(); 

            return Ok(shipment);
        }

        //Промяна на статус - Куриер
        [HttpPut("update-status/{trackingNumber}")]
        public async Task<IActionResult> UpdateStatus(string trackingNumber, [FromBody] string newStatus)
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            if (shipment == null) return NotFound(new { message = "Пратката не е намерена" });

            shipment.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Статусът е обновен успешно!" });
        }

        // Управление на пратки - оператор  
        [HttpPut("update-full/{trackingNumber}")]
        public async Task<IActionResult> UpdateShipmentDetails(string trackingNumber, [FromBody] ShipmentUpdateDto updateData)
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            if (shipment == null) return NotFound(new { message = "Пратката не е намерена" });

            if (!string.IsNullOrEmpty(updateData.Status)) shipment.Status = updateData.Status;
            shipment.Note = updateData.Note;
            shipment.ProblemType = updateData.ProblemType;
            shipment.ProblemDescription = updateData.ProblemDescription;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Данните са обновени успешно!" });
        }

        //Местене на пратка в склад - Въвеждане в склад 
        [HttpPut("move-to-warehouse/{trackingNumber}")]
        public async Task<IActionResult> MoveToWarehouse(string trackingNumber, [FromBody] WarehouseUpdateDto data)
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            
            if (shipment == null) return NotFound(new { message = "Пратката не е намерена" });

            //защита при местене на вече приключени пратки
            if (shipment.Status == "Delivered" || shipment.Status == "Refused")
            {
                return BadRequest(new { message = "Внимание: Тази пратка е приключена (Доставена или Отказана) и не може да бъде местена по складове!" });
            }
            
            // Сменя на склада
            shipment.CurrentWarehouseId = data.WarehouseId;
            shipment.Status = "In Warehouse"; 

            if (data.Weight.HasValue) shipment.Weight = data.Weight.Value;
            if (!string.IsNullOrEmpty(data.Note)) shipment.Note = data.Note;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Пратката е заведена в склада успешно!" });
        }

        //Взимане на пратки по ID на склад (за таблицата за наличност)
        [HttpGet("in-warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipmentsInWarehouse(int warehouseId)
        {
            //взимаме само пратките, които не са със статус "Delivered"
            var shipments = await _context.Shipments
                                          .Where(s => s.CurrentWarehouseId == warehouseId 
                                                   && s.Status != "Delivered" 
                                                   && s.Status != "Refused")
                                          .ToListAsync();
            return shipments;
        }

        // Получаване на статистика за пратки - мениджър
        [HttpGet("stats")]
        public async Task<ActionResult<ManagerStatsDto>> GetManagerStats()
        {
            var allShipments = await _context.Shipments.ToListAsync();

            // Взимаме всички пратки сортирани по дата, най-новите най-отгоре
            var recentShipments = await _context.Shipments
                                        .OrderByDescending(s => s.CreatedDate)
                                        .ToListAsync();

            var stats = new ManagerStatsDto
            {
                TotalShipments = allShipments.Count,
                DeliveredCount = allShipments.Count(s => s.Status == "Delivered" || s.Status == "Доставена"),
                ProblemCount = allShipments.Count(s => !string.IsNullOrEmpty(s.ProblemType)),
                InSofiaWarehouse = allShipments.Count(s => s.CurrentWarehouseId == 1),
                InVarnaWarehouse = allShipments.Count(s => s.CurrentWarehouseId == 2),
                
                // Подаваме целия списък към екрана
                LastShipments = recentShipments
            };

            return stats;
        }

        //Взимане на графика (само пратки на съответния СО) - куриер
        [HttpGet("my-schedule/{username}")]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetCourierSchedule(string username)
        {
            var courier = await _context.Couriers.FirstOrDefaultAsync(c => c.Username == username);
            
            if (courier == null) return NotFound(new { message = "Куриерът не е намерен" });

            var tasks = await _context.Shipments
                                      .Where(s => s.AssignedCourierId == courier.CourierId)
                                      .ToListAsync();
            return tasks;
        }

        //Местене на пратка в склад 
        [HttpPost("update-location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto data)
        {
            //Търси се пратката
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.TrackingNumber == data.TrackingNumber);
            
            if (shipment == null)
            {
                return NotFound(new { message = "Пратката не е намерена" });
            }

            //Блокиране на приключените пратки
            if (shipment.Status == "Delivered" || shipment.Status == "Refused")
            {
                return BadRequest(new { message = "Внимание: Тази пратка е приключена (Доставена или Отказана) и нейният статус или локация не могат да бъдат променяни!" });
            }
            

            // обновяваме, като я намери
            shipment.CurrentWarehouseId = data.WarehouseId;
            shipment.Status = "In Warehouse"; 
            
            if (!string.IsNullOrEmpty(data.Note))
            {
                shipment.Note = data.Note;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated" });
        }
    } 


    // Data Transfer Objects - DTO класове -


    public class ShipmentUpdateDto
    {
        public string? Status { get; set; }
        public string? Note { get; set; }
        public string? ProblemType { get; set; }
        public string? ProblemDescription { get; set; }
    }

    public class ShipmentCreateDto
    {
        public string SenderName { get; set; } = null!;
        public string? SenderPhone { get; set; }
        public string ReceiverName { get; set; } = null!;
        public string? ReceiverPhone { get; set; }
        public string? ReceiverAddress { get; set; }
        public decimal? Weight { get; set; }

        
        public string? CreatedBy { get; set; }
        public int? AssignedCourierId { get; set; }
        public string? Status { get; set; }
    }

    public class WarehouseUpdateDto
    {
        public int WarehouseId { get; set; }
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
    }

    public class ManagerStatsDto
    {
        public int TotalShipments { get; set; }
        public int DeliveredCount { get; set; }
        public int ProblemCount { get; set; }
        public int InSofiaWarehouse { get; set; }
        public int InVarnaWarehouse { get; set; }
        public List<Shipment> LastShipments { get; set; } = new List<Shipment>();
    }

    public class LocationUpdateDto
    {
        public string TrackingNumber { get; set; } = null!;
        public int WarehouseId { get; set; }
        public string? Note { get; set; }
    }

} 