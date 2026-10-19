// Generated from IR
import '../models/order.dart';
import '../models/order_search.dart';
import '../models/order_search_check_result.dart';
import '../presenters/porder_search_form_presenter.dart';
import '../presenters/porder_window_presenter.dart';
import '../presenters/psearch_error_message_presenter.dart';
import '../services/i_order_search.dart';
import '../services/i_order.dart';

class FindOrderUseCase {
  final POrderSearchForm _pOrderSearchForm;
  final POrderWindow _pOrderWindow;
  final PSearchErrorMessage _pSearchErrorMessage;
  final IOrderSearch _orderSearch;
  final IOrder _order;
  Order? _currentOrder;
  OrderSearch? _currentOrderSearch;
  OrderSearchCheckResult? _orderSearchCheckResult;

  FindOrderUseCase({ required POrderSearchForm pOrderSearchForm, required POrderWindow pOrderWindow, required PSearchErrorMessage pSearchErrorMessage, required IOrderSearch orderSearch, required IOrder order }) : _pOrderSearchForm = pOrderSearchForm,
      _pOrderWindow = pOrderWindow,
      _pSearchErrorMessage = pSearchErrorMessage,
      _orderSearch = orderSearch,
      _order = order;

  void execute() {}

  void handleFindOrder() {
    _hideViews();
    _pOrderSearchForm.showOrderSearchForm();
  }

  Future<void> handleSearch(OrderSearch orderSearch) async {
    _currentOrderSearch = orderSearch;
    _orderSearchCheckResult = await _orderSearch.checkOrderSearch(_currentOrderSearch!);
    if (_orderSearchCheckResult == OrderSearchCheckResult.valid) {
      _currentOrder = await _order.readOrder(_currentOrderSearch!);
      _hideViews();
      _pOrderWindow.showOrderWindow(order: _currentOrder);
    } else if (_orderSearchCheckResult == OrderSearchCheckResult.invalid) {
      _hideViews();
      _pSearchErrorMessage.showSearchErrorMessage();
    }
  }

  void handleCloseOrderWindow() {
    _end('OK');
  }

  void handleCancel() {
    _end('CANCEL');
  }

  void handleCloseSearchErrorMessage() {
    _end('notOK');
  }

  void _hideViews() {
    _pOrderSearchForm.hide();
    _pOrderWindow.hide();
    _pSearchErrorMessage.hide();
  }

  void _end(String result) {
    _hideViews();
  }
}
