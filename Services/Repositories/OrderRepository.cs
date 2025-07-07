using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly DatabaseConnectionService _dbService;

        public OrderRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Order> GetAll()
        {
            List<Order> orders = new List<Order>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllOrders");

            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(MapRowToOrder(row));
            }

            return orders;
        }

        public IEnumerable<Order> GetActiveOrders()
        {
            List<Order> orders = new List<Order>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetActiveOrders");

            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(MapRowToOrder(row));
            }

            return orders;
        }

        public Order GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@OrderId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetOrderById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToOrder(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Order order)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@UserId", SqlDbType.Int, order.UserId),
                _dbService.CreateParameter("@OrderStatus", SqlDbType.NVarChar, order.OrderStatus),
                _dbService.CreateParameter("@OrderCode", SqlDbType.NVarChar, order.OrderCode),
                _dbService.CreateParameter("@SubTotal", SqlDbType.Decimal, order.SubTotal),
                _dbService.CreateParameter("@DiscountAmount", SqlDbType.Decimal, order.DiscountAmount),
                _dbService.CreateParameter("@ShippingCost", SqlDbType.Decimal, order.ShippingCost),
                _dbService.CreateParameter("@Total", SqlDbType.Decimal, order.Total),
                _dbService.CreateParameter("@EstimatedDeliveryTime", SqlDbType.DateTime,
                    order.EstimatedDeliveryTime ?? (object)DBNull.Value),
                _dbService.CreateParameter("@OrderId", SqlDbType.Int, null)
            };

            parameters[8].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_CreateOrder", parameters);

            return (int)parameters[8].Value;
        }

        public bool Update(Order order)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@OrderId", SqlDbType.Int, order.OrderId),
                _dbService.CreateParameter("@OrderStatus", SqlDbType.NVarChar, order.OrderStatus),
                _dbService.CreateParameter("@EstimatedDeliveryTime", SqlDbType.DateTime,
                    order.EstimatedDeliveryTime ?? (object)DBNull.Value)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateOrderStatus", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@OrderId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteOrder", parameter);

            return rowsAffected > 0;
        }

        public bool CancelOrder(int orderId)
        {
            SqlParameter parameter = _dbService.CreateParameter("@OrderId", SqlDbType.Int, orderId);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_CancelOrder", parameter);

            return rowsAffected > 0;
        }

        public bool UpdateOrderStatus(int orderId, string status, DateTime? estimatedDeliveryTime = null)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@OrderId", SqlDbType.Int, orderId),
                _dbService.CreateParameter("@OrderStatus", SqlDbType.NVarChar, status),
                _dbService.CreateParameter("@EstimatedDeliveryTime", SqlDbType.DateTime,
                    estimatedDeliveryTime ?? (object)DBNull.Value)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateOrderStatus", parameters);

            // If the order status is updated to "InPreparation", update the stock quantities
            if (status == "InPreparation")
            {
                SqlParameter statusParam = _dbService.CreateParameter("@OrderId", SqlDbType.Int, orderId);
                _dbService.ExecuteNonQuery("usp_UpdateStockOnOrderStatus", statusParam);
            }

            return rowsAffected > 0;
        }

        public IEnumerable<OrderItem> GetOrderItems(int orderId)
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            SqlParameter parameter = _dbService.CreateParameter("@OrderId", SqlDbType.Int, orderId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetOrderItems", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                orderItems.Add(new OrderItem
                {
                    OrderItemId = Convert.ToInt32(row["OrderItemId"]),
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    DishId = row["DishId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DishId"]),
                    MenuId = row["MenuId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["MenuId"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(row["TotalPrice"])
                });
            }

            return orderItems;
        }

        public int AddOrderItem(OrderItem orderItem)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@OrderId", SqlDbType.Int, orderItem.OrderId),
                _dbService.CreateParameter("@DishId", SqlDbType.Int, orderItem.DishId ?? (object)DBNull.Value),
                _dbService.CreateParameter("@MenuId", SqlDbType.Int, orderItem.MenuId ?? (object)DBNull.Value),
                _dbService.CreateParameter("@Quantity", SqlDbType.Int, orderItem.Quantity),
                _dbService.CreateParameter("@UnitPrice", SqlDbType.Decimal, orderItem.UnitPrice),
                _dbService.CreateParameter("@TotalPrice", SqlDbType.Decimal, orderItem.TotalPrice),
                _dbService.CreateParameter("@OrderItemId", SqlDbType.Int, null)
            };

            parameters[6].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddOrderItem", parameters);

            return (int)parameters[6].Value;
        }

        public decimal CalculateOrderDiscount(int userId, decimal subTotal)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@UserId", SqlDbType.Int, userId),
                _dbService.CreateParameter("@SubTotal", SqlDbType.Decimal, subTotal),
                _dbService.CreateParameter("@DiscountAmount", SqlDbType.Decimal, 0)
            };

            parameters[2].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_CalculateOrderDiscount", parameters);

            return Convert.ToDecimal(parameters[2].Value);
        }

        public decimal CalculateShippingCost(decimal subTotal)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@SubTotal", SqlDbType.Decimal, subTotal),
                _dbService.CreateParameter("@ShippingCost", SqlDbType.Decimal, 0)
            };

            parameters[1].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_CalculateShippingCost", parameters);

            return Convert.ToDecimal(parameters[1].Value);
        }

        public IEnumerable<Order> GetOrdersByStatus(string status)
        {
            List<Order> orders = new List<Order>();

            SqlParameter parameter = _dbService.CreateParameter("@OrderStatus", SqlDbType.NVarChar, status);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetOrdersByStatus", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(MapRowToOrder(row));
            }

            return orders;
        }

        public IEnumerable<Order> GetUserOrdersByStatus(int userId, string status)
        {
            List<Order> orders = new List<Order>();

            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@UserId", SqlDbType.Int, userId),
                _dbService.CreateParameter("@OrderStatus", SqlDbType.NVarChar, status)
            };

            DataTable dataTable = _dbService.ExecuteReader("usp_GetUserOrdersByStatus", parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(MapRowToOrder(row));
            }

            return orders;
        }

        private Order MapRowToOrder(DataRow row)
        {
            return new Order
            {
                OrderId = Convert.ToInt32(row["OrderId"]),
                UserId = Convert.ToInt32(row["UserId"]),
                OrderDate = Convert.ToDateTime(row["OrderDate"]),
                OrderStatus = row["OrderStatus"].ToString(),
                OrderCode = row["OrderCode"].ToString(),
                SubTotal = Convert.ToDecimal(row["SubTotal"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                ShippingCost = Convert.ToDecimal(row["ShippingCost"]),
                Total = Convert.ToDecimal(row["Total"]),
                EstimatedDeliveryTime = row["EstimatedDeliveryTime"] == DBNull.Value ?
                    (DateTime?)null : Convert.ToDateTime(row["EstimatedDeliveryTime"])
            };
        }
    }
}