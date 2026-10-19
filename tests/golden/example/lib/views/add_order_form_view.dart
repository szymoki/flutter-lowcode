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
  final _numberController = TextEditingController();
  final _titleController = TextEditingController();
  final _datesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<PAddOrderForm>().show();
    });
  }

  @override
  void dispose() {
    _numberController.dispose();
    _titleController.dispose();
    _datesController.dispose();
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
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _numberController, decoration: const InputDecoration(labelText: 'Number', border: OutlineInputBorder()), keyboardType: TextInputType.number,)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _titleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _datesController, decoration: const InputDecoration(labelText: 'Dates', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () => widget.controller.onAdd(), child: const Text('add')),
              ],
            ),
          ),
        );
      },
    );
  }
}
