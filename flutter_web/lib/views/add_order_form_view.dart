// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/padd_order_form_presenter.dart';
import '../controllers/add_order_form_controller.dart';

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
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<PAddOrderForm>().show();
    });
  }

  @override
  void dispose() {
    _orderNumberController.dispose();
    _orderTitleController.dispose();
    _orderDatesController.dispose();
    super.dispose();
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
                ElevatedButton(onPressed: () => widget.controller.onAdd(), child: const Text('add')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
