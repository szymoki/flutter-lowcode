// Generated from IR
import '../use_cases/find_order_use_case.dart';

class COrderSearchForm {
  final FindOrderUseCase _findOrderUseCase;

  COrderSearchForm({required FindOrderUseCase findOrderUseCase}) : _findOrderUseCase = findOrderUseCase;

  void onSearch() { _findOrderUseCase.handleSearch(); }
}
