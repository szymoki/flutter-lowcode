// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/padd_order_form_presenter.dart';
import '../controllers/add_order_form_controller.dart';
import '../models/order.dart';

class AddOrderFormView extends StatefulWidget {
  final CAddOrderForm controller;

  const AddOrderFormView({super.key, required this.controller});

  @override
  State<AddOrderFormView> createState() => _AddOrderFormViewState();
}

class _AddOrderFormViewState extends State<AddOrderFormView> {
  final _orderNumberController = TextEditingController();
  final _orderTitleController = TextEditingController();
  final _orderDatesController = TextEditingController();

  @override
  void dispose() {
    _orderNumberController.dispose();
    _orderTitleController.dispose();
    _orderDatesController.dispose();
    super.dispose();
  }

  Order? _buildOrder() {
    final number = int.tryParse(_orderNumberController.text);
    if (number == null) {
      _showInvalidValue('Number');
      return null;
    }
    return Order(number: number, title: _orderTitleController.text, dates: _orderDatesController.text);
  }

  void _showInvalidValue(String field) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Nieprawidłowa wartość pola $field')));
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<PAddOrderForm>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('AddOrderForm')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: ListView(
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _orderNumberController, decoration: const InputDecoration(labelText: 'Number', border: OutlineInputBorder()), keyboardType: TextInputType.number,)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _orderTitleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _orderDatesController, decoration: const InputDecoration(labelText: 'Dates', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () { final order = _buildOrder(); if (order == null) return; widget.controller.onAdd(order); }, child: const Text('add')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
