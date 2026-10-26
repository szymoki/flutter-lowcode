// Generated from IR
import 'order.dart';

class OrderList {
  final List<Order> orders;

  const OrderList({
    required this.orders
  });

  OrderList copyWith({List<Order>? orders}) {
    return OrderList(
      orders: orders ?? this.orders
    );
  }
}
