// Generated from IR
import '../models/order_search.dart';
import '../use_cases/find_order_use_case.dart';

class COrderSearchForm {
  final FindOrderUseCase _findOrderUseCase;

  COrderSearchForm({required FindOrderUseCase findOrderUseCase}) : _findOrderUseCase = findOrderUseCase;

  void onSearch(OrderSearch orderSearch) { _findOrderUseCase.handleSearch(orderSearch); }
  void onCancel() { _findOrderUseCase.handleCancel(); }
}
