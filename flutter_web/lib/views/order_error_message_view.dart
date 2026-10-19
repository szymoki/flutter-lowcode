// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_error_message_presenter.dart';
import '../controllers/order_error_message_controller.dart';

class OrderErrorMessageView extends StatefulWidget {
  final COrderErrorMessage controller;

  const OrderErrorMessageView({super.key, required this.controller});

  @override
  State<OrderErrorMessageView> createState() => _OrderErrorMessageViewState();
}

class _OrderErrorMessageViewState extends State<OrderErrorMessageView> {
  final _numberController = TextEditingController();
  final _titleController = TextEditingController();
  final _datesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderErrorMessage>().show();
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
    return Consumer<POrderErrorMessage>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderErrorMessage')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _numberController, decoration: const InputDecoration(labelText: 'Number', border: OutlineInputBorder()), keyboardType: TextInputType.number,)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _titleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _datesController, decoration: const InputDecoration(labelText: 'Dates', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () => widget.controller.onClose(), child: const Text('close')),
                ElevatedButton(onPressed: () => widget.controller.onCorrect(), child: const Text('correct')),
              ],
            ),
          ),
        );
      },
    );
  }
}
