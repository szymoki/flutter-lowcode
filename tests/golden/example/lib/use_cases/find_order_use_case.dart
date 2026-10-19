// Generated from IR
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

  FindOrderUseCase({ required POrderSearchForm pOrderSearchForm, required POrderWindow pOrderWindow, required PSearchErrorMessage pSearchErrorMessage, required IOrderSearch orderSearch, required IOrder order }) : _pOrderSearchForm = pOrderSearchForm,
      _pOrderWindow = pOrderWindow,
      _pSearchErrorMessage = pSearchErrorMessage,
      _orderSearch = orderSearch,
      _order = order;

  void execute() {}
  void handleClose() {}
  void handleSearch() {}
  void handleCancel() {}
  void handleShowOrderList() {}
}
