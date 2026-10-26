// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_search_form_presenter.dart';
import '../controllers/order_search_form_controller.dart';
import '../models/order_search.dart';

class OrderSearchFormView extends StatefulWidget {
  final COrderSearchForm controller;

  const OrderSearchFormView({super.key, required this.controller});

  @override
  State<OrderSearchFormView> createState() => _OrderSearchFormViewState();
}

class _OrderSearchFormViewState extends State<OrderSearchFormView> {
  final _orderSearchTitleController = TextEditingController();

  @override
  void dispose() {
    _orderSearchTitleController.dispose();
    super.dispose();
  }

  OrderSearch? _buildOrderSearch() {
    return OrderSearch(title: _orderSearchTitleController.text);
  }


  @override
  Widget build(BuildContext context) {
    return Consumer<POrderSearchForm>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderSearchForm')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: ListView(
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _orderSearchTitleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () { final orderSearch = _buildOrderSearch(); if (orderSearch == null) return; widget.controller.onSearch(orderSearch); }, child: const Text('search')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
