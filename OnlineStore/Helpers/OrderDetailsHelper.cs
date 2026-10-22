using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using System.Windows;

namespace OnlineStore.Helpers
{
    public static class OrderDetailsHelper
    {
        public static void Show(int idOrder)
        {
            if (App.CurrentUser == null) return;

            try
            {
                var db = App.Context;
                var order = db.Orders.Include(p => p.IdUserNavigation).Include(p => p.IdOrderStatusNavigation).Include(p => p.OrderItems).ThenInclude(p => p.IdProductNavigation).FirstOrDefault(p => p.IdOrder == idOrder);

                if (order == null)
                {
                    MessageBox.Show("Заказ не найден.");
                    return;
                }

                var user = db.Users.Include(p => p.IdRoleNavigation).FirstOrDefault(p => p.IdUser == App.CurrentUser.IdUser);
                if (user == null) return;
                string role = user.IdRoleNavigation.NameRole.ToLower();
                bool staff = role.Contains("администратор") || role.Contains("менеджер");

                if (order.IdUser != user.IdUser && !staff)
                {
                    MessageBox.Show("Нет доступа к этому заказу.");
                    return;
                }

                string details = $"Заказ №{order.IdOrder}\nПокупатель: {order.IdUserNavigation.Login}\n";
                details += $"Дата: {order.DateOrder:dd.MM.yyyy HH:mm}\nСтатус: {order.IdOrderStatusNavigation.NameOrderStatus}\n";
                details += $"Адрес: {order.DeliveryAddress}\n\n";

                decimal total = 0;
                foreach (var item in order.OrderItems)
                {
                    details += $"{item.IdProductNavigation.NameProduct}: {item.QuantityOrderItem} шт. × {item.PriceOrderItem:N2} руб.\n";
                    total += item.QuantityOrderItem * item.PriceOrderItem;
                }

                details += $"\nИтого: {total:N2} руб.";
                MessageBox.Show(details, "Состав заказа");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть заказ: " + ex.Message);
            }
        }
    }
}
