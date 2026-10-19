// Generated from IR
import '../use_cases/find_order_use_case.dart';
import '../use_cases/show_order_list_use_case.dart';

class CSearchErrorMessage {
  final FindOrderUseCase _findOrderUseCase;
  final ShowOrderListUseCase _showOrderListUseCase;

  CSearchErrorMessage({required FindOrderUseCase findOrderUseCase, required ShowOrderListUseCase showOrderListUseCase}) : _findOrderUseCase = findOrderUseCase, _showOrderListUseCase = showOrderListUseCase;

  void onClose() { _findOrderUseCase.handleCloseSearchErrorMessage(); }
  void onShowOrderList() { _showOrderListUseCase.handleShowOrderList(); }
}
