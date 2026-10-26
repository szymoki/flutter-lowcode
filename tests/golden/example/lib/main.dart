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
import 'app.dart';

void main() {
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => PMainMenu()),
        ChangeNotifierProvider(create: (_) => POrderSearchForm()),
        ChangeNotifierProvider(create: (_) => POrderWindow()),
        ChangeNotifierProvider(create: (_) => PSearchErrorMessage()),
        ChangeNotifierProvider(create: (_) => POrderListWindow()),
        ChangeNotifierProvider(create: (_) => PAddOrderForm()),
        ChangeNotifierProvider(create: (_) => POrderSuccessMessage()),
        ChangeNotifierProvider(create: (_) => POrderErrorMessage()),
        ChangeNotifierProvider(create: (_) => PDeleteConfirmationMessage()),
        ChangeNotifierProvider(create: (_) => POrderDeletedMessage()),
      ],
      child: MaterialApp(
        title: 'Generated App',
        home: const App(),
      ),
    ),
  );
}
