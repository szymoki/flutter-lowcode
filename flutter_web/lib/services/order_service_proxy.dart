// Generated from IR - stub for developer
import 'i_order.dart';
import '../models/order_search.dart';
import '../models/order_check_result.dart';
import '../models/order.dart';


class OrderServiceProxy implements IOrder {
  @override
  Future<Future<dynamic>> readOrder(OrderSearch orderSearch) async {
    throw UnimplementedError();
  }

  @override
  Future<OrderCheckResult> checkOrder(Order order) async {
    throw UnimplementedError();
  }

  @override
  Future<Future<void>> updateOrder(Order order) async {
    throw UnimplementedError();
  }

  @override
  Future<Future<void>> deleteOrder() async {
    throw UnimplementedError();
  }

}
