using Auto.Data;
using Auto.Data.Entities;
using Auto.Website.Messages;
using Auto.Website.Models;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Auto.Website.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly IAutoDatabase db;
        private readonly IBus _bus;

        public OwnersController(IAutoDatabase db, IBus bus)
        {
            this.db = db;
            _bus = bus;
        }

        private dynamic Paginate(string url, int index, int count, int total)
        {
            dynamic links = new ExpandoObject();
            links.self = new { href = url };
            links.final = new { href = $"{url}?index={total - (total % count)}&count={count}" };
            links.first = new { href = $"{url}?index=0&count={count}" };
            if (index > 0) links.previous = new { href = $"{url}?index={index - count}&count={count}" };
            if (index + count < total) links.next = new { href = $"{url}?index={index + count}&count={count}" };
            return links;
        }

        // GET: api/owners
        [HttpGet]
        [Produces("application/hal+json")]
        public IActionResult Get(int index = 0, int count = 10)
        {
            var items = db.ListOwners().Skip(index).Take(count);
            var total = db.CountOwners();
            var _links = Paginate("/api/owners", index, count, total);
            var _actions = new
            {
                create = new
                {
                    method = "POST",
                    type = "application/json",
                    name = "Create a new owner",
                    href = "/api/owners"
                },
                delete = new
                {
                    method = "DELETE",
                    name = "Delete a owner",
                    href = "/api/owners/{id}"
                }
            };
            var result = new
            {
                _links,
                _actions,
                index,
                count,
                total,
                items
            };
            return Ok(result);
        }

        // GET api/vehicles
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var owner = db.FindOwner(id);
            if (owner == default) return NotFound();
            var json = owner.ToDynamic();
            json._links = new
            {
                self = new { href = $"/api/owners/{id}" },
                ownerVehicle = new { href = $"/api/vehicles/{owner.VehicleCode}" }
            };
            json._actions = new
            {
                update = new
                {
                    method = "PUT",
                    href = $"/api/owners/{id}",
                    accept = "application/json"
                },
                delete = new
                {
                    method = "DELETE",
                    href = $"/api/owners/{id}"
                }
            };

            return Ok(json);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OwnerDto dto)
        {
            var owner = new Owner
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                VehicleCode = dto.VehicleCode
            };
            db.CreateOwner(owner);
            PublishNewOwnerMessage(owner);
            return Ok(dto);
        }
        private void PublishNewOwnerMessage(Owner owner)
        {
            var message = new NewOwnerMessage()
            {
                Email = owner.Email,
                LastName = owner.LastName,
                FirstName = owner.FirstName,
                VehicleCode = owner.VehicleCode,
                ListedAtUtc = DateTime.UtcNow
            };
            _bus.PubSub.Publish(message);
        }


        // PUT api/vehicles
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] OwnerDto dto)
        {

            var ownerVehicle = db.FindOwner(id);
            var owner = new Owner
            {
                Email = id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                VehicleCode = dto.VehicleCode
            };
            db.UpdateOwner(owner);
            return Get(id);
        }

        // DELETE api/vehicles
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            db.DeleteOwner(id);
            return Ok();
        }
    }
}
