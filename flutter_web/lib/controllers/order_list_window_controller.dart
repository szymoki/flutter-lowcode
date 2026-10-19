// Generated from IR
import '../use_cases/show_order_list_use_case.dart';
import '../use_cases/find_order_use_case.dart';
import '../use_cases/add_order_use_case.dart';
import '../use_cases/delete_order_use_case.dart';

class COrderListWindow {
  final ShowOrderListUseCase _showOrderListUseCase;
  final FindOrderUseCase _findOrderUseCase;
  final AddOrderUseCase _addOrderUseCase;
  final DeleteOrderUseCase _deleteOrderUseCase;

  COrderListWindow({required ShowOrderListUseCase showOrderListUseCase, required FindOrderUseCase findOrderUseCase, required AddOrderUseCase addOrderUseCase, required DeleteOrderUseCase deleteOrderUseCase}) : _showOrderListUseCase = showOrderListUseCase, _findOrderUseCase = findOrderUseCase, _addOrderUseCase = addOrderUseCase, _deleteOrderUseCase = deleteOrderUseCase;

  void onClose() { _showOrderListUseCase.handleClose(); }
  void onOrderList() { _showOrderListUseCase.handleOrderList(); }
  void onFindOrder() { _findOrderUseCase.handleFindOrder(); }
  void onAddOrder() { _addOrderUseCase.handleAddOrder(); }
  void onDeleteOrder() { _deleteOrderUseCase.handleDeleteOrder(); }
}
