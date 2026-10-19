// Generated from IR
import '../use_cases/find_order_use_case.dart';

class COrderSearchForm {
  final FindOrderUseCase _useCase;

  COrderSearchForm({required FindOrderUseCase useCase}) : _useCase = useCase;

  void onSearch() { _useCase.handleSearch(); }
}
