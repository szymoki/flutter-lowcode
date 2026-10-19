// Generated from IR
import '../models/order_search.dart';
import '../models/order_check_result.dart';
import '../models/order.dart';

abstract class IOrder {
  Future<Future<dynamic>> readOrder(OrderSearch orderSearch);
  Future<OrderCheckResult> checkOrder(Order order);
  Future<Future<void>> updateOrder(Order order);
  Future<Future<void>> deleteOrder(Order order);
}
