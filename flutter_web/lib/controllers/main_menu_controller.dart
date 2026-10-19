// Generated from IR
import '../use_cases/show_order_list_use_case.dart';
import '../use_cases/find_order_use_case.dart';
import '../use_cases/add_order_use_case.dart';

class CMainMenu {
  final ShowOrderListUseCase _showOrderListUseCase;
  final FindOrderUseCase _findOrderUseCase;
  final AddOrderUseCase _addOrderUseCase;

  CMainMenu({required ShowOrderListUseCase showOrderListUseCase, required FindOrderUseCase findOrderUseCase, required AddOrderUseCase addOrderUseCase}) : _showOrderListUseCase = showOrderListUseCase, _findOrderUseCase = findOrderUseCase, _addOrderUseCase = addOrderUseCase;

  void onShowOrderList() { _showOrderListUseCase.handleShowOrderList(); }
  void onFindOrder() { _findOrderUseCase.handleFindOrder(); }
  void onAddOrder() { _addOrderUseCase.handleAddOrder(); }
}
