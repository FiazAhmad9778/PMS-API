using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.API.Core.Domain.Interfaces.Services;
public interface ICascadeDeleteService
{
  Task DeleteClientDrivers(long clientId);
  Task DeleteClient(long clientId, long clientGroupId);
  Task DeleteWarehouseUsers(long warehouseId);
  Task DeleteClientGroup(long clientGroupId);
  Task DeleteUser(long userId);
}
