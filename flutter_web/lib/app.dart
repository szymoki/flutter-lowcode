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
  const App({super.key});

  @override
  State<App> createState() => _AppState();
}

class _AppState extends State<App> {
  StartUseCase? _startUseCase;
  FindOrderUseCase? _findOrderUseCase;
  ShowOrderListUseCase? _showOrderListUseCase;
  AddOrderUseCase? _addOrderUseCase;
  DeleteOrderUseCase? _deleteOrderUseCase;
  late CMainMenu _cMainMenu;
  late COrderSearchForm _cOrderSearchForm;
  late COrderWindow _cOrderWindow;
  late CSearchErrorMessage _cSearchErrorMessage;
  late COrderListWindow _cOrderListWindow;
  late CAddOrderForm _cAddOrderForm;
  late COrderSuccessMessage _cOrderSuccessMessage;
  late COrderErrorMessage _cOrderErrorMessage;
  late CDeleteConfirmationMessage _cDeleteConfirmationMessage;
  late COrderDeletedMessage _cOrderDeletedMessage;

  @override
  Widget build(BuildContext context) {
    if (_startUseCase == null) {
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
            _cMainMenu = CMainMenu(showOrderListUseCase: _showOrderListUseCase!, findOrderUseCase: _findOrderUseCase!, addOrderUseCase: _addOrderUseCase!);
      _cOrderSearchForm = COrderSearchForm(findOrderUseCase: _findOrderUseCase!);
      _cOrderWindow = COrderWindow(findOrderUseCase: _findOrderUseCase!);
      _cSearchErrorMessage = CSearchErrorMessage(findOrderUseCase: _findOrderUseCase!);
      _cOrderListWindow = COrderListWindow(showOrderListUseCase: _showOrderListUseCase!, findOrderUseCase: _findOrderUseCase!, addOrderUseCase: _addOrderUseCase!, deleteOrderUseCase: _deleteOrderUseCase!);
      _cAddOrderForm = CAddOrderForm(addOrderUseCase: _addOrderUseCase!);
      _cOrderSuccessMessage = COrderSuccessMessage(addOrderUseCase: _addOrderUseCase!);
      _cOrderErrorMessage = COrderErrorMessage(addOrderUseCase: _addOrderUseCase!);
      _cDeleteConfirmationMessage = CDeleteConfirmationMessage(deleteOrderUseCase: _deleteOrderUseCase!);
      _cOrderDeletedMessage = COrderDeletedMessage(deleteOrderUseCase: _deleteOrderUseCase!);
      WidgetsBinding.instance.addPostFrameCallback((_) => setState(() {}));
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }
    return Scaffold(
      appBar: AppBar(title: const Text('Use cases')),
      body: ListView(
        children: [
            ListTile(title: const Text('MainMenu'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => MainMenuView(controller: _cMainMenu)))),
            ListTile(title: const Text('OrderSearchForm'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderSearchFormView(controller: _cOrderSearchForm)))),
            ListTile(title: const Text('OrderWindow'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderWindowView(controller: _cOrderWindow)))),
            ListTile(title: const Text('SearchErrorMessage'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => SearchErrorMessageView(controller: _cSearchErrorMessage)))),
            ListTile(title: const Text('OrderListWindow'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderListWindowView(controller: _cOrderListWindow)))),
            ListTile(title: const Text('AddOrderForm'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => AddOrderFormView(controller: _cAddOrderForm)))),
            ListTile(title: const Text('OrderSuccessMessage'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderSuccessMessageView(controller: _cOrderSuccessMessage)))),
            ListTile(title: const Text('OrderErrorMessage'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderErrorMessageView(controller: _cOrderErrorMessage)))),
            ListTile(title: const Text('DeleteConfirmationMessage'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => DeleteConfirmationMessageView(controller: _cDeleteConfirmationMessage)))),
            ListTile(title: const Text('OrderDeletedMessage'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => OrderDeletedMessageView(controller: _cOrderDeletedMessage)))),
        ],
      ),
    );
  }
}
