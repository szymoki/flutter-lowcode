// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'presenters/pmain_menu_presenter.dart';
import 'presenters/porder_search_form_presenter.dart';
import 'presenters/porder_window_presenter.dart';
import 'presenters/psearch_error_message_presenter.dart';
import 'presenters/porder_list_window_presenter.dart';
import 'presenters/padd_order_form_presenter.dart';
import 'presenters/porder_success_message_presenter.dart';
import 'presenters/porder_error_message_presenter.dart';
import 'presenters/pdelete_confirmation_message_presenter.dart';
import 'presenters/porder_deleted_message_presenter.dart';
import 'presenters/view_presenter.dart';
import 'use_cases/start_use_case.dart';
import 'use_cases/find_order_use_case.dart';
import 'use_cases/show_order_list_use_case.dart';
import 'use_cases/add_order_use_case.dart';
import 'use_cases/delete_order_use_case.dart';
import 'controllers/main_menu_controller.dart';
import 'controllers/order_search_form_controller.dart';
import 'controllers/order_window_controller.dart';
import 'controllers/search_error_message_controller.dart';
import 'controllers/order_list_window_controller.dart';
import 'controllers/add_order_form_controller.dart';
import 'controllers/order_success_message_controller.dart';
import 'controllers/order_error_message_controller.dart';
import 'controllers/delete_confirmation_message_controller.dart';
import 'controllers/order_deleted_message_controller.dart';
import 'views/main_menu_view.dart';
import 'views/order_search_form_view.dart';
import 'views/order_window_view.dart';
import 'views/search_error_message_view.dart';
import 'views/order_list_window_view.dart';
import 'views/add_order_form_view.dart';
import 'views/order_success_message_view.dart';
import 'views/order_error_message_view.dart';
import 'views/delete_confirmation_message_view.dart';
import 'views/order_deleted_message_view.dart';
import 'services/order_search_service_proxy.dart';
import 'services/order_service_proxy.dart';
import 'services/order_list_service_proxy.dart';

class App extends StatefulWidget {
  /// Tryb diagnostyczny: lista wszystkich widoków zamiast przebiegu od przypadku użycia Start.
  final bool viewListMode;

  const App({super.key, this.viewListMode = false});

  @override
  State<App> createState() => _AppState();
}

class _AppState extends State<App> {
  late final StartUseCase _startUseCase;
  late final FindOrderUseCase _findOrderUseCase;
  late final ShowOrderListUseCase _showOrderListUseCase;
  late final AddOrderUseCase _addOrderUseCase;
  late final DeleteOrderUseCase _deleteOrderUseCase;
  late final CMainMenu _cMainMenu;
  late final COrderSearchForm _cOrderSearchForm;
  late final COrderWindow _cOrderWindow;
  late final CSearchErrorMessage _cSearchErrorMessage;
  late final COrderListWindow _cOrderListWindow;
  late final CAddOrderForm _cAddOrderForm;
  late final COrderSuccessMessage _cOrderSuccessMessage;
  late final COrderErrorMessage _cOrderErrorMessage;
  late final CDeleteConfirmationMessage _cDeleteConfirmationMessage;
  late final COrderDeletedMessage _cOrderDeletedMessage;
  late final List<ViewPresenter> _viewPresenters;
  bool _initialized = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_initialized) return;
    _initialized = true;

    final pMainMenu = context.read<PMainMenu>();
    final pOrderSearchForm = context.read<POrderSearchForm>();
    final pOrderWindow = context.read<POrderWindow>();
    final pSearchErrorMessage = context.read<PSearchErrorMessage>();
    final pOrderListWindow = context.read<POrderListWindow>();
    final pAddOrderForm = context.read<PAddOrderForm>();
    final pOrderSuccessMessage = context.read<POrderSuccessMessage>();
    final pOrderErrorMessage = context.read<POrderErrorMessage>();
    final pDeleteConfirmationMessage = context.read<PDeleteConfirmationMessage>();
    final pOrderDeletedMessage = context.read<POrderDeletedMessage>();
    final orderSearch = OrderSearchServiceProxy();
    final order = OrderServiceProxy();
    final orderList = OrderListServiceProxy();
    _startUseCase = StartUseCase(pMainMenu: pMainMenu);
    _findOrderUseCase = FindOrderUseCase(pOrderSearchForm: pOrderSearchForm, pOrderWindow: pOrderWindow, pSearchErrorMessage: pSearchErrorMessage, orderSearch: orderSearch, order: order);
    _showOrderListUseCase = ShowOrderListUseCase(pOrderListWindow: pOrderListWindow, orderList: orderList);
    _addOrderUseCase = AddOrderUseCase(pAddOrderForm: pAddOrderForm, pOrderSuccessMessage: pOrderSuccessMessage, pOrderErrorMessage: pOrderErrorMessage, order: order);
    _deleteOrderUseCase = DeleteOrderUseCase(pDeleteConfirmationMessage: pDeleteConfirmationMessage, pOrderDeletedMessage: pOrderDeletedMessage, order: order);

    _cMainMenu = CMainMenu(showOrderListUseCase: _showOrderListUseCase, startUseCase: _startUseCase, findOrderUseCase: _findOrderUseCase, addOrderUseCase: _addOrderUseCase);
    _cOrderSearchForm = COrderSearchForm(findOrderUseCase: _findOrderUseCase);
    _cOrderWindow = COrderWindow(findOrderUseCase: _findOrderUseCase);
    _cSearchErrorMessage = CSearchErrorMessage(findOrderUseCase: _findOrderUseCase);
    _cOrderListWindow = COrderListWindow(showOrderListUseCase: _showOrderListUseCase, findOrderUseCase: _findOrderUseCase, addOrderUseCase: _addOrderUseCase, deleteOrderUseCase: _deleteOrderUseCase);
    _cAddOrderForm = CAddOrderForm(addOrderUseCase: _addOrderUseCase);
    _cOrderSuccessMessage = COrderSuccessMessage(addOrderUseCase: _addOrderUseCase);
    _cOrderErrorMessage = COrderErrorMessage(addOrderUseCase: _addOrderUseCase);
    _cDeleteConfirmationMessage = CDeleteConfirmationMessage(deleteOrderUseCase: _deleteOrderUseCase);
    _cOrderDeletedMessage = COrderDeletedMessage(deleteOrderUseCase: _deleteOrderUseCase);
    _viewPresenters = [pMainMenu, pOrderSearchForm, pOrderWindow, pSearchErrorMessage, pOrderListWindow, pAddOrderForm, pOrderSuccessMessage, pOrderErrorMessage, pDeleteConfirmationMessage, pOrderDeletedMessage];

    if (!widget.viewListMode) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _startUseCase.handleApplication());
    }
  }

  /// Indeks widoku na wierzchu: widocznego i wyświetlonego najpóźniej.
  int _topViewIndex() {
    var top = -1;
    for (var i = 0; i < _viewPresenters.length; i++) {
      final p = _viewPresenters[i];
      if (p.isVisible && (top < 0 || p.shownAt > _viewPresenters[top].shownAt)) top = i;
    }
    return top;
  }

  @override
  Widget build(BuildContext context) {
    if (widget.viewListMode) return _buildViewList(context);
    return ListenableBuilder(
      listenable: Listenable.merge(_viewPresenters),
      builder: (context, _) {
        final index = _topViewIndex();
        if (index < 0) {
          return const Scaffold(body: Center(child: CircularProgressIndicator()));
        }
        return IndexedStack(
          index: index,
          children: [
            MainMenuView(controller: _cMainMenu),
            OrderSearchFormView(controller: _cOrderSearchForm),
            OrderWindowView(controller: _cOrderWindow),
            SearchErrorMessageView(controller: _cSearchErrorMessage),
            OrderListWindowView(controller: _cOrderListWindow),
            AddOrderFormView(controller: _cAddOrderForm),
            OrderSuccessMessageView(controller: _cOrderSuccessMessage),
            OrderErrorMessageView(controller: _cOrderErrorMessage),
            DeleteConfirmationMessageView(controller: _cDeleteConfirmationMessage),
            OrderDeletedMessageView(controller: _cOrderDeletedMessage),
          ],
        );
      },
    );
  }

  Widget _buildViewList(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Views')),
      body: ListView(
        children: [
          ListTile(title: const Text('MainMenu'), onTap: () => _openForDiagnostics(context, 0, MainMenuView(controller: _cMainMenu))),
          ListTile(title: const Text('OrderSearchForm'), onTap: () => _openForDiagnostics(context, 1, OrderSearchFormView(controller: _cOrderSearchForm))),
          ListTile(title: const Text('OrderWindow'), onTap: () => _openForDiagnostics(context, 2, OrderWindowView(controller: _cOrderWindow))),
          ListTile(title: const Text('SearchErrorMessage'), onTap: () => _openForDiagnostics(context, 3, SearchErrorMessageView(controller: _cSearchErrorMessage))),
          ListTile(title: const Text('OrderListWindow'), onTap: () => _openForDiagnostics(context, 4, OrderListWindowView(controller: _cOrderListWindow))),
          ListTile(title: const Text('AddOrderForm'), onTap: () => _openForDiagnostics(context, 5, AddOrderFormView(controller: _cAddOrderForm))),
          ListTile(title: const Text('OrderSuccessMessage'), onTap: () => _openForDiagnostics(context, 6, OrderSuccessMessageView(controller: _cOrderSuccessMessage))),
          ListTile(title: const Text('OrderErrorMessage'), onTap: () => _openForDiagnostics(context, 7, OrderErrorMessageView(controller: _cOrderErrorMessage))),
          ListTile(title: const Text('DeleteConfirmationMessage'), onTap: () => _openForDiagnostics(context, 8, DeleteConfirmationMessageView(controller: _cDeleteConfirmationMessage))),
          ListTile(title: const Text('OrderDeletedMessage'), onTap: () => _openForDiagnostics(context, 9, OrderDeletedMessageView(controller: _cOrderDeletedMessage))),
        ],
      ),
    );
  }

  void _openForDiagnostics(BuildContext context, int index, Widget view) {
    _viewPresenters[index].show();
    Navigator.push(context, MaterialPageRoute(builder: (_) => view));
  }
}
