using DummyProject.DataStorage.Data;
using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.DataStorage.Repository
{
    public class ShopingCartRepository:Repository<ShopingCart>,IShopingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShopingCartRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }
    }
}
