// Generated from IR
import '../models/order.dart';
import '../models/order_check_result.dart';
import '../models/order_search.dart';

abstract class IOrder {
  Future<Order> readOrder(OrderSearch orderSearch);
  Future<OrderCheckResult> checkOrder(Order order);
  Future<void> updateOrder(Order order);
  Future<void> deleteOrder(Order order);
}
