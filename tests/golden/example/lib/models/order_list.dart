// Generated from IR
import 'order.dart';

class OrderList {
  final Order orders;

  const OrderList({
    required this.orders
  });

  OrderList copyWith({Order? orders}) {
    return OrderList(
      orders: orders ?? this.orders
    );
  }
}
